package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import com.phonebrowser.app.services.pairing.PairingEntry
import com.phonebrowser.app.services.pairing.PairingManager
import android.app.Application
import android.content.Context
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject

@HiltViewModel
class HomeViewModel @Inject constructor(
    @ApplicationContext private val context: Context
) : ViewModel() {
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
        PhoneBrowserForegroundService.stopDiscovery(context)
        logEntries.add(0, "Sparowano z ${it.requesterName}")
    }
    fun rejectPairing() = pairingRequest?.let { PairingManager.reject(it.requestId) }
}