package com.phonebrowser.app.storage

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "trusted_devices")
data class TrustedDeviceEntity(
    @PrimaryKey val deviceId: String,
    val deviceName: String,
    val pairingToken: String,
    val lastConnectedAt: Long
)
