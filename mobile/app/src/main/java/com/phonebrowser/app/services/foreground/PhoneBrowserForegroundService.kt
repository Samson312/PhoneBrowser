package com.phonebrowser.app.services.foreground

import android.app.*
import android.content.Context
import android.content.Intent
import android.content.pm.ServiceInfo
import android.os.Build
import android.os.IBinder
import androidx.core.app.NotificationCompat
import androidx.core.app.ServiceCompat
import com.phonebrowser.app.MainActivity
import com.phonebrowser.app.services.http.PhoneBrowserHttpServer
import com.phonebrowser.app.services.discovery.UdpDiscoveryService
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.*
import timber.log.Timber
import javax.inject.Inject


@AndroidEntryPoint
class PhoneBrowserForegroundService: Service() {

    @Inject lateinit var httpServer: PhoneBrowserHttpServer
    @Inject lateinit var udpDiscovery: UdpDiscoveryService

    private val serviceScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        if (intent?.action == ACTION_STOP_DISCOVERY) {
            Timber.i("Stopping discovery by request")
            udpDiscovery.stop()
            return START_NOT_STICKY
        }

        try {
            ServiceCompat.startForeground(this, NOTIFICATION_ID, buildNotification(), serviceType())
        } catch (e: Exception) {
            Timber.e(e, "Failed to start foreground service")
            stopSelf()
            return START_NOT_STICKY
        }

        httpServer.start()
        udpDiscovery.startBroadcasting(serviceScope)
        Timber.i("PhoneBrowser foreground service started")

        return START_NOT_STICKY
    }

    override fun onTaskRemoved(rootIntent: Intent?) {
        super.onTaskRemoved(rootIntent)
        Timber.d("Task removed, stopping service")
        stopSelf()
    }

    override fun onDestroy() {
        Timber.i("PhoneBrowser foreground service stopping")
        udpDiscovery.stop()
        httpServer.stop()
        serviceScope.cancel()
        super.onDestroy()
    }

    override fun onBind(intent: Intent?): IBinder? = null

    private fun serviceType(): Int =
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q)
            ServiceInfo.FOREGROUND_SERVICE_TYPE_DATA_SYNC
        else
            0

    private fun buildNotification(): Notification {
        val channelId = "phonebrowser_discovery"

        val channel = NotificationChannel(
            channelId,
            "Widoczność w sieci",
            NotificationManager.IMPORTANCE_LOW
        )
        getSystemService(NotificationManager::class.java)
            .createNotificationChannel(channel)

        val openApp = PendingIntent.getActivity(
            this, 0,
            Intent(this, MainActivity::class.java),
            PendingIntent.FLAG_IMMUTABLE
        )

        return NotificationCompat.Builder(this, channelId)
            .setContentTitle("Telefon widoczny dla komputerów — dotknij, by wyłączyć")
            .setSmallIcon(android.R.drawable.stat_sys_download)
            .setContentIntent(openApp)
            .setOngoing(true)
            .build()
    }

    companion object {
        private const val NOTIFICATION_ID = 1001
        private const val ACTION_STOP_DISCOVERY = "com.phonebrowser.app.STOP_DISCOVERY"

        fun start(context: Context) {
            val intent = Intent(context, PhoneBrowserForegroundService::class.java)
            context.startForegroundService(intent)
        }

        fun stop(context: Context) {
            context.stopService(Intent(context, PhoneBrowserForegroundService::class.java))
        }

        fun stopDiscovery(context: Context){
            val intent = Intent(context, PhoneBrowserForegroundService::class.java)
            .setAction(ACTION_STOP_DISCOVERY)
            context.startService(intent)
        }
    }
}