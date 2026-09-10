package com.phonebrowser.app.services.http

import com.phonebrowser.app.services.http.routes.pairingEndpoints
import com.phonebrowser.app.services.http.routes.photoEndpoints
import com.phonebrowser.app.services.media.PhotoRepository
import com.phonebrowser.app.services.pairing.PairingManager
import com.phonebrowser.app.services.session.ActiveSessionService
import com.phonebrowser.app.storage.TrustedDeviceDao
import io.ktor.http.*
import io.ktor.serialization.kotlinx.json.*
import io.ktor.server.application.*
import io.ktor.server.auth.Authentication
import io.ktor.server.auth.UserIdPrincipal
import io.ktor.server.auth.authenticate
import io.ktor.server.auth.bearer
import io.ktor.server.cio.*
import io.ktor.server.engine.*
import io.ktor.server.plugins.contentnegotiation.*
import io.ktor.server.response.*
import io.ktor.server.routing.*
import io.ktor.server.plugins.statuspages.*

import kotlinx.serialization.json.Json
import javax.inject.Inject
import javax.inject.Named

class PhoneBrowserHttpServer @Inject constructor(
    @Named("httpPort") private val port: Int,
    private val photoRepository: PhotoRepository,
    private val trustedDeviceDao: TrustedDeviceDao,
    private val activeSession: ActiveSessionService,
    private val pairingManager: PairingManager
) {

    private var server: EmbeddedServer<*, *>? = null

    fun start() {
        if (server != null) return
        server = embeddedServer(CIO, port = port) {
            install(StatusPages) {
                exception<Throwable> { call, cause ->
                    cause.printStackTrace()
                    call.respondText("Error: ${cause.message}", status = HttpStatusCode.InternalServerError)
                }
            }
            install(ContentNegotiation) {
                json(Json { ignoreUnknownKeys = true })
            }
            install(Authentication) {
                bearer("auth-bearer") {
                    realm = "PhoneBrowser"
                    authenticate { tokenCredential ->
                        val peer = activeSession.connectedPeer.value

                        if (peer?.pairingToken == tokenCredential.token) {
                            UserIdPrincipal(peer.deviceId)
                        } else {
                            null
                        }
                    }
                }
            }

            routing {
                get("/health") {
                    call.respondText("OK")
                }

                pairingEndpoints(pairingManager)

                authenticate("auth-bearer"){
                    photoEndpoints(photoRepository)
                }
            }
        }.start(wait = false)
    }

    fun stop() {
        server?.stop(gracePeriodMillis = 500, timeoutMillis = 1000)
        server = null
    }
}