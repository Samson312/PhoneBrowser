package com.phonebrowser.app.services.discovery

import com.phonebrowser.app.models.DiscoveryMessage
import com.phonebrowser.app.storage.SettingsRepository
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.first
import org.json.JSONException
import timber.log.Timber
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.SocketException
import java.net.SocketTimeoutException
import javax.inject.Inject
import javax.inject.Named

class UdpDiscoveryService @Inject constructor(
    @Named("httpPort") private val httpPort: Int,
    private val settingsRepository: SettingsRepository
){
    private var job: Job? = null

    private val port:Int = 47821

    fun startBroadcasting(scope: CoroutineScope){
        if(job?.isActive == true) return

        job = scope.launch(Dispatchers.IO) {
            val socket = try {
                DatagramSocket(port).apply { soTimeout = 1000 }
            } catch (e: SocketException) {
                Timber.e(e, "Failed to bind UDP socket on port %d", port)
                return@launch
            }

            try {
                while (isActive) {
                    val buffer = ByteArray(1024)
                    val packet = DatagramPacket(buffer, buffer.size)

                    try {
                        socket.receive(packet)
                    } catch (e: SocketTimeoutException) {
                        continue
                    } catch (e: SocketException) {
                        Timber.w(e, "UDP receive failed, continuing listen loop")
                        continue
                    }

                    val raw = String(packet.data, 0, packet.length)


                    val message = try {
                        DiscoveryMessage.fromJson(raw)
                    } catch (e: JSONException) {
                        Timber.w(e, "Received malformed discovery message: %s", raw)
                        null
                    }

                    if (message?.type != "DISCOVER") {
                        message?.let { Timber.d("Ignored message of type %s", it.type) }
                        continue
                    }

                    Timber.d("Discovery request from %s: %s", packet.address, message.deviceName)

                    val settings = settingsRepository.settingsFlow.first()

                    var reply = DiscoveryMessage(
                        deviceId = settings.deviceId,
                        deviceName = settings.deviceName,
                        httpPort = httpPort
                    )

                    try {
                        val data = reply.toJson().toByteArray()
                        socket.send(DatagramPacket(data, data.size, packet.address, packet.port))
                        Timber.d("Sent ANNOUNCE to %s", packet.address)
                    } catch (e: SocketException) {
                        Timber.w(e, "Failed to send ANNOUNCE reply to %s", packet.address)
                    }
                }
            } catch (e: CancellationException) {
                throw e
            } catch (e: Exception) {
                Timber.e(e, "Unexpected error in discovery listen loop")
            } finally {
                socket.close()
                Timber.d("Discovery socket closed")
            }
        }
    }

    fun stop() {
        job?.cancel()
        Timber.d("Discovery stopped")
    }
}