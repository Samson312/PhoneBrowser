package com.phonebrowser.app.ui.home

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.phonebrowser.app.viewmodels.HomeViewModel

@Composable
fun HomeScreen(viewModel: HomeViewModel = viewModel()) {
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
            onDismissRequest = {},
            title = { Text("${pairingRequest.requesterName} chce się połączyć") },
            text = { Text("To urządzenie chce przesyłać i pobierać zdjęcia z tego telefonu.") },
            confirmButton = { TextButton(onClick = { viewModel.acceptPairing() }) { Text("Akceptuj") } },
            dismissButton = { TextButton(onClick = { viewModel.rejectPairing() }) { Text("Odrzuć") } }
        )
    }
}