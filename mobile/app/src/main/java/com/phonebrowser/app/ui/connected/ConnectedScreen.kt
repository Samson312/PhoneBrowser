package com.phonebrowser.app.ui.connected

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.phonebrowser.app.ui.components.ScreenLayout
import com.phonebrowser.app.viewmodels.ConnectedViewModel

@Composable
fun ConnectedScreen(viewModel: ConnectedViewModel = hiltViewModel()){
    val peer by viewModel.connectedPeer.collectAsStateWithLifecycle()

    val name = peer?.deviceName ?: return

    ScreenLayout {
        Text(
            text="Połączono z:",
            style = MaterialTheme.typography.titleLarge)
        Spacer(Modifier.height(8.dp))
        Text(
            text = name,
            style = MaterialTheme.typography.headlineLarge,
            textAlign = TextAlign.Center)
        Spacer(Modifier.height(18.dp))
        Text(
            text = "Zdjęcia wybierzesz i zapiszesz na komputerze. Na telefonie nie musisz nic już robić.",
            style = MaterialTheme.typography.bodyLarge,
            textAlign = TextAlign.Center
        )
    }
}