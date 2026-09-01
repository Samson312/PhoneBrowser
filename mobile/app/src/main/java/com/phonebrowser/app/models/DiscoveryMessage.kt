package com.phonebrowser.app.models

import org.json.JSONObject

data class DiscoveryMessage(val type: String, val deviceName: String? = null, val tcpPort: Int? = null){
    fun toJson(): String = JSONObject().apply {
        put("type", type)
        deviceName?.let { put("deviceName", it) }
        tcpPort?.let { put("tcpPort", it) }
    }.toString()

    companion object {
        fun fromJson(raw: String): DiscoveryMessage {
            val obj = JSONObject(raw)
            return DiscoveryMessage(
                type = obj.getString("Type"),
                deviceName = if (obj.has("DeviceName")) obj.getString("DeviceName") else null,
                tcpPort = if (obj.has("TcpPort")) obj.getInt("TcpPort") else null
            )
        }
    }
}
