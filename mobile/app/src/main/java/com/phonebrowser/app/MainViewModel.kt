package com.phonebrowser.app

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.compose.runtime.mutableStateListOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.discovery.UdpDiscoveryService

class MainViewModel : ViewModel() {
    var statusText by mutableStateOf("Gotowy")
        private set

    val logEntries = mutableStateListOf<String>()

    private val discoveryService = UdpDiscoveryService()

    fun startDiscovery() {
        statusText = "Rozgłaszanie w sieci..."
        discoveryService.startBroadcasting(viewModelScope) { entry ->
            logEntries.add(0, entry)
        }
    }

    fun stopDiscovery() {
        discoveryService.stop()
        statusText = "Zatrzymano"
    }
}