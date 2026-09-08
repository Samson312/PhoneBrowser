package com.phonebrowser.app.viewmodels

import android.content.Context
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import com.phonebrowser.app.services.pairing.PairingEntry
import com.phonebrowser.app.services.pairing.PairingManager
import com.phonebrowser.app.services.session.ActiveSessionService
import com.phonebrowser.app.storage.TrustedDeviceDao
import com.phonebrowser.app.storage.TrustedDeviceEntity
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class PairingViewModel @Inject constructor(
    @ApplicationContext private val context: Context,
    private val pairingManager: PairingManager,
    private val activeSessionService: ActiveSessionService,
    private val trustedDeviceDao: TrustedDeviceDao
) : ViewModel() {
    var pairingRequest by mutableStateOf<PairingEntry?>(null)
        private set

    init {
        pairingManager.incomingRequest
            .onEach { pairingRequest = it }
            .launchIn(viewModelScope)
    }

    fun acceptPairing(){
        val entry = pairingRequest ?: return
        val accepted = pairingManager.accept(entry.requestId) ?: return

        viewModelScope.launch {
            val trustedDevice = saveTrustedDevice(accepted)
            activeSessionService.setConnected(trustedDevice)
            PhoneBrowserForegroundService.stopDiscovery(context)
        }
    }

    fun rejectPairing() = pairingRequest?.let { pairingManager.reject(it.requestId) }

    private suspend fun saveTrustedDevice(accepted: PairingEntry): TrustedDeviceEntity{

        var trustedDevice = TrustedDeviceEntity(
            deviceId = accepted.requesterDeviceId,
            deviceName = accepted.requesterName,
            pairingToken = accepted.token.orEmpty(),
            lastConnectedAt = System.currentTimeMillis()
        )

        try {
            trustedDeviceDao.upsert(trustedDevice)
        } catch (e: Exception) {
            throw e
        }
        return trustedDevice
    }
}