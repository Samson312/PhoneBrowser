package com.phonebrowser.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.phonebrowser.app.ui.theme.PhoneBrowserMobileTheme
import com.phonebrowser.app.viewmodels.MainViewModel

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            PhoneBrowserMobileTheme {
                MainScreen()
            }
        }
    }
}

@Composable
fun MainScreen(viewModel: MainViewModel = viewModel()) {
    val pairingRequest = viewModel.pairingRequest

    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Spacer(modifier = Modifier.height(16.dp))
        LazyColumn {
            items(viewModel.logEntries) { entry ->
                Text(entry, style = MaterialTheme.typography.bodySmall)
            }
        }
    }

    if (pairingRequest != null) {
        AlertDialog(
            onDismissRequest = { /* zgodnie z zasadą 6/9: wymagamy jawnej decyzji */ },
            title = { Text("${pairingRequest.requesterName} chce się połączyć") },
            text = { Text("To urządzenie chce przesyłać i pobierać zdjęcia z tego telefonu.") },
            confirmButton = { TextButton(onClick = { viewModel.acceptPairing() }) { Text("Akceptuj") } },
            dismissButton = { TextButton(onClick = { viewModel.rejectPairing() }) { Text("Odrzuć") } }
        )
    }
}