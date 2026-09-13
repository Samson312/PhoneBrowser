package com.phonebrowser.app.storage

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.emptyPreferences
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import com.phonebrowser.app.models.SettingsProfile
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.catch
import kotlinx.coroutines.flow.map
import java.io.IOException
import timber.log.Timber
import java.util.UUID
import javax.inject.Singleton

private val Context.dataStore by preferencesDataStore(name = "settings")

@Singleton
class SettingsRepository @Inject constructor(
    @ApplicationContext private val context: Context
) {
    private object Keys{
        val DEVICE_ID = stringPreferencesKey("device_id")
        val DEVICE_NAME = stringPreferencesKey("device_name")
    }

    val hasProfileFlow: Flow<Boolean> = context.dataStore.data
        .catch { e ->
        if (e is IOException) {
            Timber.e(e, "Failed to read settings DataStore, treating as no profile")
            emit(emptyPreferences())
        } else throw e
        }
        .map { prefs -> prefs[Keys.DEVICE_NAME] != null}

    val settingsFlow: Flow<SettingsProfile> = context.dataStore.data
        .catch { e ->
            if (e is IOException) {
                Timber.e(e, "Failed to read settings DataStore, falling back to defaults")
                emit(emptyPreferences())
            } else throw e
        }
        .map { prefs ->
            SettingsProfile(
                deviceId = prefs[Keys.DEVICE_ID] ?: UUID.randomUUID().toString(),
                deviceName = prefs[Keys.DEVICE_NAME] ?: "Telefon"
            )
        }

    suspend fun saveDeviceName(name: String) {
        try {
            context.dataStore.edit { prefs ->
                if (prefs[Keys.DEVICE_ID] == null) {
                    prefs[Keys.DEVICE_ID] = UUID.randomUUID().toString()
                }
                prefs[Keys.DEVICE_NAME] = name
            }
            Timber.i("Saved device name")
        } catch (e: IOException) {
            Timber.e(e, "Failed to save device name to DataStore")
            throw e
        }
    }
}