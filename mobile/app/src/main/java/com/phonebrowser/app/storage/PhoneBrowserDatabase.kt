package com.phonebrowser.app.storage

import androidx.room.Database
import androidx.room.RoomDatabase

@Database(
    entities = [TrustedDeviceEntity::class],
    version = 1,
    exportSchema = true
)

abstract class PhoneBrowserDatabase : RoomDatabase() {
    abstract fun trustedDeviceDao(): TrustedDeviceDao
}