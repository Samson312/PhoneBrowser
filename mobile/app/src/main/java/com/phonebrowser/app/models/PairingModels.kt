package com.phonebrowser.app.models

import kotlinx.serialization.Serializable

@Serializable
data class DeviceInfoDto(
    val deviceId: String,
    val deviceName: String,
    val platform: String,
    val protocolVersion: Int,
)

@Serializable
data class PairingRequestDto(
    val requestId: String,
    val requester: DeviceInfoDto
)

@Serializable
data class PairingStatusResponseDto(
    val requestId: String,
    val status: String,           // Pending | Accepted | Rejected | Expired
    val pairingToken: String? = null
)