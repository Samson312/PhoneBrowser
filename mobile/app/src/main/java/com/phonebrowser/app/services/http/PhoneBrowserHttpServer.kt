package com.phonebrowser.app.services.http

import com.phonebrowser.app.services.http.routes.pairingEndpoints
import io.ktor.http.*
import io.ktor.serialization.kotlinx.json.*
import io.ktor.server.application.*
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
    @Named("httpPort") private val port: Int
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
            routing {
                get("/health") {
                    call.respondText("OK")
                }

                pairingEndpoints()

            }
        }.start(wait = false)
    }

    fun stop() {
        server?.stop(gracePeriodMillis = 500, timeoutMillis = 1000)
        server = null
    }
}