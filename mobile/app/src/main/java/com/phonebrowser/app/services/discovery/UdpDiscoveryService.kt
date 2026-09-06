package com.phonebrowser.app.services.discovery

import com.phonebrowser.app.models.DiscoveryMessage
import com.phonebrowser.app.storage.SettingsRepository
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.first
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.SocketTimeoutException
import javax.inject.Inject
import javax.inject.Named

class UdpDiscoveryService @Inject constructor(
    @Named("httpPort") private val httpPort: Int,
    private val settingsRepository: SettingsRepository
){
    private var job: Job? = null

    private val port:Int = 47821

    fun startBroadcasting(scope: CoroutineScope, onLog: (String) -> Unit){
        if(job?.isActive == true) return

        job = scope.launch(Dispatchers.IO) {
            val socket = DatagramSocket(port).apply{
                soTimeout = 1000
            }

            try {
                while (isActive) {
                    val buffer = ByteArray(1024)
                    val packet = DatagramPacket(buffer, buffer.size)

                    try {
                        socket.receive(packet)
                    } catch (e: SocketTimeoutException) {
                        continue
                    }

                    val raw = String(packet.data, 0, packet.length)


                    val message = try {
                        DiscoveryMessage.fromJson(raw)
                    } catch (e: Exception) {
                        withContext(Dispatchers.Main) { onLog("Nieprawidłowa wiadomość: $raw") }
                        null
                    }

                    if (message?.type != "DISCOVER") {
                        if (message != null) {
                            withContext(Dispatchers.Main) { onLog("Zignorowano: ${message.type}") }
                        }
                        continue
                    }

                    withContext(Dispatchers.Main) {
                        onLog("Znaleziono ${message.deviceName}: ${packet.address}")
                    }

                    val settings = settingsRepository.settingsFlow.first()

                    var reply = DiscoveryMessage(
                        deviceId = settings.deviceId,
                        deviceName = settings.deviceName,
                        httpPort = httpPort
                    )

                    var data = reply.toJson().toByteArray()

                    socket.send(DatagramPacket(data, data.size, packet.address, packet.port))
                    withContext(Dispatchers.Main) { onLog("Wysłano: ${reply.type}|${message.httpPort}") }
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) { onLog("Błąd: ${e.message}") }
            } finally {
                socket.close()
            }
        }
    }

    fun stop() {
        job?.cancel()
    }
}