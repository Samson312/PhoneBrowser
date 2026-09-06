package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import com.phonebrowser.app.services.pairing.PairingEntry
import com.phonebrowser.app.services.pairing.PairingManager
import android.app.Application
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach

class HomeViewModel(application: Application) : AndroidViewModel(application) {
    val logEntries = mutableStateListOf<String>()

    var pairingRequest by mutableStateOf<PairingEntry?>(null)
        private set

    init {
        PairingManager.incomingRequest
            .onEach { pairingRequest = it }
            .launchIn(viewModelScope)
    }

    fun acceptPairing() = pairingRequest?.let {
        PairingManager.accept(it.requestId)
        PhoneBrowserForegroundService.stopDiscovery(getApplication())
        logEntries.add(0, "Sparowano z ${it.requesterName}")
    }
    fun rejectPairing() = pairingRequest?.let { PairingManager.reject(it.requestId) }
}