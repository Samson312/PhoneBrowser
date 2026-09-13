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
import timber.log.Timber
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

        try{
            server = embeddedServer(CIO, port = port) {
                install(StatusPages) {
                    exception<Throwable> { call, cause ->
                        Timber.e(cause, "Unhandled exception while processing %s", call.request.local.uri)
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
                                Timber.w("Rejected request with invalid or missing bearer token")
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

            Timber.i("HTTP server started on port %d", port)
        }
        catch (e: Exception) {
            Timber.e(e, "Failed to start HTTP server on port %d", port)
            server = null
        }
    }

    fun stop() {
        try {
            server?.stop(gracePeriodMillis = 500, timeoutMillis = 1000)
            Timber.i("HTTP server stopped")
        } catch (e: Exception) {
            Timber.w(e, "Error while stopping HTTP server")
        } finally {
            server = null
        }
    }
}