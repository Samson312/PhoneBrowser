package com.phonebrowser.app.services.http

import com.phonebrowser.app.models.PairingRequestDto
import com.phonebrowser.app.models.PairingStatusResponseDto
import com.phonebrowser.app.services.pairing.PairingManager
import io.ktor.http.*
import io.ktor.serialization.kotlinx.json.*
import io.ktor.server.application.*
import io.ktor.server.cio.*
import io.ktor.server.engine.*
import io.ktor.server.plugins.contentnegotiation.*
import io.ktor.server.request.*
import io.ktor.server.response.*
import io.ktor.server.routing.*
import io.ktor.server.plugins.statuspages.*

import kotlinx.serialization.json.Json

class PhoneBrowserHttpServer(private val port: Int) {

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

                post("/pairing/request") {
                    val body = call.receive<PairingRequestDto>()
                    PairingManager.receiveRequest(
                        requestId = body.requestId,
                        requesterDeviceId = body.requester.deviceId,
                        requesterName = body.requester.deviceName
                    )
                    // Wg architektury: 202 natychmiast, bez czekania na decyzję użytkownika
                    call.respond(
                        HttpStatusCode.Accepted,
                        PairingStatusResponseDto(requestId = body.requestId, status = "Pending")
                    )
                }

                get("/pairing/status/{requestId}") {
                    val requestId = call.parameters["requestId"]
                    val entry = requestId?.let { PairingManager.getStatus(it) }
                    if (entry == null) {
                        call.respond(HttpStatusCode.NotFound)
                        return@get
                    }
                    call.respond(
                        PairingStatusResponseDto(
                            requestId = entry.requestId,
                            status = entry.status.name,
                            pairingToken = entry.token
                        )
                    )
                }
            }
        }.start(wait = false)
    }

    fun stop() {
        server?.stop(gracePeriodMillis = 500, timeoutMillis = 1000)
        server = null
    }
}