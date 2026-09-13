package com.phonebrowser.app

import android.Manifest
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.core.content.ContextCompat
import com.phonebrowser.app.ui.theme.PhoneBrowserMobileTheme
import com.phonebrowser.app.ui.nav.AppNavigation
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import dagger.hilt.android.AndroidEntryPoint
import timber.log.Timber

@AndroidEntryPoint
class MainActivity : ComponentActivity() {

    private val permissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ){ granted ->
        if (granted) {
            Timber.i("Media permission granted, starting foreground service")
            PhoneBrowserForegroundService.start(this)
        } else {
            Timber.w("Media permission denied — app cannot list or serve photos")
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val permission = if (Build.VERSION.SDK_INT >= 33)
            Manifest.permission.READ_MEDIA_IMAGES
        else
            Manifest.permission.READ_EXTERNAL_STORAGE

        if (ContextCompat.checkSelfPermission(this, permission) == PackageManager.PERMISSION_GRANTED) {
            PhoneBrowserForegroundService.start(this)
        } else {
            permissionLauncher.launch(permission)
        }

        setContent {
            PhoneBrowserMobileTheme {
                AppNavigation()
            }
        }
    }
}