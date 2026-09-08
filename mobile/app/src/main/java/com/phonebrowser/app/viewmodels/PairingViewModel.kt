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
    private val trustedDeviceDao: TrustedDeviceDao
) : ViewModel() {
    var pairingRequest by mutableStateOf<PairingEntry?>(null)
        private set

    init {
        PairingManager.incomingRequest
            .onEach { pairingRequest = it }
            .launchIn(viewModelScope)
    }

    fun acceptPairing(){
        val entry = pairingRequest ?: return
        val accepted = PairingManager.accept(entry.requestId) ?: return

        saveTrustedDevice(accepted)

        PhoneBrowserForegroundService.stopDiscovery(context)
    }

    fun rejectPairing() = pairingRequest?.let { PairingManager.reject(it.requestId) }

    private fun saveTrustedDevice(accepted: PairingEntry){
        viewModelScope.launch {
            trustedDeviceDao.upsert(
                TrustedDeviceEntity(
                    deviceId = accepted.requesterDeviceId,
                    deviceName = accepted.requesterName,
                    platform = "Windows",
                    pairingToken = accepted.token.orEmpty(),
                    lastConnectedAt = System.currentTimeMillis()
                )
            )
        }
    }
}