package com.phonebrowser.app.services.discovery

import android.os.Build
import com.phonebrowser.app.models.DiscoveryMessage
import kotlinx.coroutines.*
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress
import java.net.SocketTimeoutException

class UdpDiscoveryService(private val port: Int = 2000) {
    private var job: Job? = null

    fun startBroadcasting(scope: CoroutineScope, onLog: (String) -> Unit){
        job = scope.launch(Dispatchers.IO) {
            val socket = DatagramSocket().apply {
                broadcast = true
                soTimeout = 2000
            }

            val broadcastAddress = InetAddress.getByName("255.255.255.255")
            val broadcastMessage = DiscoveryMessage(type = "BROADCAST", deviceName = Build.BRAND)

            val data = broadcastMessage.toJson().toByteArray()

            try {
                while (isActive) {
                    socket.send(DatagramPacket(data, data.size, broadcastAddress, port))
                    withContext(Dispatchers.Main) { onLog("Wysłano: ${broadcastMessage.deviceName}") }

                    val buffer = ByteArray(1024)
                    val replyPacket = DatagramPacket(buffer, buffer.size)
                    try {
                        socket.receive(replyPacket)
                        val raw = String(replyPacket.data, 0, replyPacket.length)

                        val message = try {
                            DiscoveryMessage.fromJson(raw)
                        } catch (e: Exception) {
                            withContext(Dispatchers.Main) { onLog("Nieprawidłowa wiadomość: $raw") }
                            null
                        }

                        if (message?.type == "PAIR_REPLY" && message.tcpPort != null) {
                            withContext(Dispatchers.Main) {
                                onLog("Znaleziono desktop: ${replyPacket.address}:${message.tcpPort}")
                            }
                            break
                        } else if (message != null) {
                            withContext(Dispatchers.Main) { onLog("Zignorowano: ${message.type}") }
                        }
                    } catch (e: SocketTimeoutException) { }
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