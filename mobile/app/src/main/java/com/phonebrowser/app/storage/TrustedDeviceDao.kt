package com.phonebrowser.app.storage

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import kotlinx.coroutines.flow.Flow

@Dao
interface TrustedDeviceDao {

    @Query("SELECT * FROM trusted_devices ORDER BY deviceName")
    fun getAll(): Flow<List<TrustedDeviceEntity>>

    @Query("SELECT * FROM trusted_devices WHERE deviceId = :deviceId")
    suspend fun getById(deviceId: String): TrustedDeviceEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(device: TrustedDeviceEntity)

    @Query("DELETE FROM trusted_devices WHERE deviceId = :deviceId")
    suspend fun deleteById(deviceId: String): Int
}