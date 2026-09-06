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
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import javax.inject.Inject

@HiltViewModel
class PairingViewModel @Inject constructor(
    @ApplicationContext private val context: Context
) : ViewModel() {
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
    }

    fun rejectPairing() = pairingRequest?.let { PairingManager.reject(it.requestId) }
}