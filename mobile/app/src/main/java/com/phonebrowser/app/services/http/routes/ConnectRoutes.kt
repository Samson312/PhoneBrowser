package com.phonebrowser.app.services.http.routes

import com.phonebrowser.app.services.session.ActiveSessionService
import com.phonebrowser.app.storage.TrustedDeviceDao
import io.ktor.http.HttpStatusCode
import io.ktor.server.routing.Route
import io.ktor.server.auth.UserIdPrincipal
import io.ktor.server.response.respond
import io.ktor.server.routing.post
import io.ktor.server.auth.principal


fun Route.connectEndpoint(trustedDeviceDao: TrustedDeviceDao, activeSession: ActiveSessionService){
    post("/connect"){
        val deviceId = call.principal<UserIdPrincipal>()?.name
            ?: return@post call.respond(HttpStatusCode.Unauthorized)

        val device = trustedDeviceDao.getById(deviceId)
            ?: return@post call.respond(HttpStatusCode.Unauthorized)

        if(activeSession.isConnected){
            if(activeSession.connectedPeer.value?.deviceId != device.deviceId){
                return@post call.respond(HttpStatusCode.Conflict)
            }

            return@post call.respond(HttpStatusCode.OK)
        }

        activeSession.setConnected(device)

        // TODO: Refresh lastConnectedAt of connecting device and upsert into db

        call.respond(HttpStatusCode.OK)
    }
}

fun Route.disconnectEndpoint(activeSession: ActiveSessionService){
    post("/disconnect"){
        val deviceId = call.principal<UserIdPrincipal>()?.name
            ?: return@post call.respond(HttpStatusCode.Unauthorized)

        if(activeSession.connectedPeer.value?.deviceId != deviceId){
            return@post call.respond(HttpStatusCode.Unauthorized)
        }

        activeSession.clear()

        return@post call.respond(HttpStatusCode.OK)
    }
}