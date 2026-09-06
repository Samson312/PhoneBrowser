package com.phonebrowser.app.ui.pairing

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import com.phonebrowser.app.viewmodels.PairingViewModel

@Composable
fun PairingScreen(
        onHandled: () -> Unit,
        viewModel: PairingViewModel = hiltViewModel()
) {
        val request = viewModel.pairingRequest ?: return

        Column(
                modifier = Modifier.fillMaxSize().padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.SpaceBetween
        ) {
                Column(
                        modifier = Modifier.weight(1f),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                ) {
                        Text(request.requesterName)
                        Spacer(Modifier.height(8.dp))
                        Text("chce się połączyć z tym telefonem, żeby przesyłać zdjęcia.")
                        Spacer(modifier = Modifier.height(16.dp))
                }

                Column(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.Center
                ) {
                        Button(
                                onClick = {
                                viewModel.acceptPairing()
                                onHandled()
                        }) { Text("Akceptuj") }

                        Spacer(modifier = Modifier.height(8.dp))

                        Button(
                                onClick = {
                                viewModel.rejectPairing()
                                onHandled()
                        }) { Text("Odrzuć") }
                }
        }
}

