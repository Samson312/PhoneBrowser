package com.phonebrowser.app.services.pairing

import com.phonebrowser.app.services.session.ActiveSessionService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import timber.log.Timber
import java.security.SecureRandom
import java.util.concurrent.ConcurrentHashMap
import javax.inject.Singleton
import javax.inject.Inject

enum class PairingStatus { PENDING, ACCEPTED, REJECTED, EXPIRED }

data class PairingEntry(
    val requestId: String,
    val requesterDeviceId: String,
    val requesterName: String,
    var status: PairingStatus = PairingStatus.PENDING,
    var token: String? = null
)

@Singleton
class PairingManager @Inject constructor(
    private val activeSessionService: ActiveSessionService
) {
    private val entries = ConcurrentHashMap<String, PairingEntry>()

    private val _incomingRequest = MutableStateFlow<PairingEntry?>(null)
    val incomingRequest: StateFlow<PairingEntry?> = _incomingRequest

    fun receiveRequest(requestId: String, requesterDeviceId: String, requesterName: String): PairingEntry? {
        if (activeSessionService.isConnected) {
            Timber.i("Ignoring pairing request from %s — already connected", requesterName)
            return null
        }

        val entry = PairingEntry(requestId, requesterDeviceId, requesterName)
        entries[requestId] = entry
        _incomingRequest.value = entry
        Timber.d("Pairing request %s queued for user confirmation", requestId)
        return entry
    }

    fun getStatus(requestId: String): PairingEntry? = entries[requestId]

    fun accept(requestId: String): PairingEntry? {
        val entry = entries[requestId] ?: run {
            Timber.w("accept() called for unknown request %s", requestId)
            return null
        }
        val tokenBytes = ByteArray(32).also { SecureRandom().nextBytes(it) }
        entry.token = tokenBytes.joinToString("") { "%02x".format(it) }
        entry.status = PairingStatus.ACCEPTED
        clearIfCurrent(requestId)
        Timber.d("Pairing %s accepted for %s", requestId, entry.requesterName)
        return entry
    }

    fun reject(requestId: String): PairingEntry? {
        val entry = entries[requestId] ?: run {
            Timber.w("reject() called for unknown request %s", requestId)
            return null
        }
        entry.status = PairingStatus.REJECTED
        clearIfCurrent(requestId)
        Timber.d("Pairing %s rejected for %s", requestId, entry.requesterName)
        return entry
    }

    private fun clearIfCurrent(requestId: String) {
        if (_incomingRequest.value?.requestId == requestId) _incomingRequest.value = null
    }
}