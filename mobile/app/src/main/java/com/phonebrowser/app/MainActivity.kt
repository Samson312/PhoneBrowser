package com.phonebrowser.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.phonebrowser.app.ui.theme.PhoneBrowserMobileTheme
import com.phonebrowser.app.ui.nav.AppNavigation
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        PhoneBrowserForegroundService.start(this)
        setContent {
            PhoneBrowserMobileTheme {
                AppNavigation()
            }
        }
    }
}