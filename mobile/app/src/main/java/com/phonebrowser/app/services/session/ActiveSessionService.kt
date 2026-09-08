package com.phonebrowser.app.services.session

import com.phonebrowser.app.storage.TrustedDeviceEntity
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ActiveSessionService @Inject constructor(){
    private val _connectedPeer = MutableStateFlow<TrustedDeviceEntity?>(null)
    val connectedPeer: StateFlow<TrustedDeviceEntity?> = _connectedPeer

    val isConnected: Boolean
        get() = _connectedPeer.value != null

    fun setConnected(peer: TrustedDeviceEntity) {
        _connectedPeer.value = peer
    }

    fun clear() {
        _connectedPeer.value = null
    }
}