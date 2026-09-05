package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.discovery.UdpDiscoveryService
import com.phonebrowser.app.services.http.PhoneBrowserHttpServer
import com.phonebrowser.app.services.pairing.PairingEntry
import com.phonebrowser.app.services.pairing.PairingManager
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import java.util.UUID

class MainViewModel : ViewModel() {
    val logEntries = mutableStateListOf<String>()
    private val httpPort: Int = 8787

    var pairingRequest by mutableStateOf<PairingEntry?>(null)
        private set

    private val httpServer = PhoneBrowserHttpServer(httpPort)
    private val discoveryService = UdpDiscoveryService(httpPort)

    init {
        PairingManager.incomingRequest
            .onEach { pairingRequest = it }
            .launchIn(viewModelScope)

        startListening()
    }

    fun startListening() {
        logEntries.add(0, "Rozpoczęto")

        httpServer.start()

        discoveryService.startBroadcasting(viewModelScope) { entry ->
            logEntries.add(0, entry)
        }
    }

    fun stopDiscovery() {
        discoveryService.stop()
        httpServer.stop()
    }

    fun acceptPairing() = pairingRequest?.let {
        PairingManager.accept(it.requestId)
        logEntries.add(0, "Sparowano z ${it.requesterName}")
        discoveryService.stop()
    }
    fun rejectPairing() = pairingRequest?.let { PairingManager.reject(it.requestId) }

    override fun onCleared() {
        discoveryService.stop()
        httpServer.stop()
    }
}