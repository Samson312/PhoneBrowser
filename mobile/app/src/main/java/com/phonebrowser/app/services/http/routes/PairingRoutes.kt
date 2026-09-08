package com.phonebrowser.app.services.http.routes

import com.phonebrowser.app.models.PairingRequestDto
import com.phonebrowser.app.models.PairingStatusResponseDto
import com.phonebrowser.app.services.pairing.PairingManager
import io.ktor.http.HttpStatusCode
import io.ktor.server.request.receive
import io.ktor.server.response.respond
import io.ktor.server.routing.Route
import io.ktor.server.routing.get
import io.ktor.server.routing.post

fun Route.pairingEndpoints() {
    post("/pairing/request") {
        val body = call.receive<PairingRequestDto>()
        PairingManager.receiveRequest(
            requestId = body.requestId,
            requesterDeviceId = body.requester.deviceId,
            requesterName = body.requester.deviceName
        )

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