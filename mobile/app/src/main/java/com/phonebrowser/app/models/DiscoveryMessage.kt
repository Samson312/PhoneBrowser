package com.phonebrowser.app.models

import org.json.JSONObject

data class DiscoveryMessage(val deviceId: String, var deviceName: String, var httpPort: Int){
    var type: String = "ANNOUNCE"
    var platform: String = "Android"
    var protocolVersion: Int = 1
    fun toJson(): String = JSONObject().apply {
        put("deviceId", deviceId)
        put("deviceName", deviceName)
        put("httpPort", httpPort)
        put("type", type)
        put("platform", platform)
        put("protocolVersion", protocolVersion)
    }.toString()
    companion object {
        fun fromJson(raw: String): DiscoveryMessage {
            val obj = JSONObject(raw)
            return DiscoveryMessage(
                deviceId = obj.getString("deviceId"),
                deviceName = obj.getString("deviceName"),
                httpPort = obj.optInt("httpPort", 0))
                .apply {
                    type = obj.optString("type", "")
                    platform = obj.optString("platform", "")
                    protocolVersion = obj.optInt("protocolVersion", 1)
                }
        }
    }
}
