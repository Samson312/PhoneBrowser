package com.phonebrowser.app.services.pairing

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import java.security.SecureRandom
import java.util.concurrent.ConcurrentHashMap

enum class PairingStatus { PENDING, ACCEPTED, REJECTED, EXPIRED }

data class PairingEntry(
    val requestId: String,
    val requesterDeviceId: String,
    val requesterName: String,
    var status: PairingStatus = PairingStatus.PENDING,
    var token: String? = null
)

object PairingManager {
    private val entries = ConcurrentHashMap<String, PairingEntry>()

    private val _incomingRequest = MutableStateFlow<PairingEntry?>(null)
    val incomingRequest: StateFlow<PairingEntry?> = _incomingRequest

    fun receiveRequest(requestId: String, requesterDeviceId: String, requesterName: String): PairingEntry {
        val entry = PairingEntry(requestId, requesterDeviceId, requesterName)
        entries[requestId] = entry
        _incomingRequest.value = entry
        return entry
    }

    fun getStatus(requestId: String): PairingEntry? = entries[requestId]

    fun accept(requestId: String): PairingEntry? {
        val entry = entries[requestId] ?: return null
        val tokenBytes = ByteArray(32).also { SecureRandom().nextBytes(it) }
        entry.token = tokenBytes.joinToString("") { "%02x".format(it) }
        entry.status = PairingStatus.ACCEPTED
        clearIfCurrent(requestId)
        return entry
    }

    fun reject(requestId: String): PairingEntry? {
        val entry = entries[requestId] ?: return null
        entry.status = PairingStatus.REJECTED
        clearIfCurrent(requestId)
        return entry
    }

    private fun clearIfCurrent(requestId: String) {
        if (_incomingRequest.value?.requestId == requestId) _incomingRequest.value = null
    }
}