package com.phonebrowser.app.ui.pairing

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import com.phonebrowser.app.ui.components.ScreenLayout
import com.phonebrowser.app.ui.components.PrimaryButton
import com.phonebrowser.app.ui.components.SecondaryButton
import com.phonebrowser.app.viewmodels.PairingViewModel

@Composable
fun PairingScreen(
        onHandled: () -> Unit,
        viewModel: PairingViewModel = hiltViewModel()
) {
        val request = viewModel.pairingRequest ?: return

        ScreenLayout(
                bottomBar = {
                        PrimaryButton(
                                text = "Akceptuj",
                                onClick = {
                                        viewModel.acceptPairing()
                                        onHandled()
                                }
                        )
                        Spacer(Modifier.height(12.dp))
                        SecondaryButton(
                                text = "Odrzuć",
                                onClick = {
                                        viewModel.rejectPairing()
                                        onHandled()
                                }
                        )
                }
        ) {
                Text(
                        text= request.requesterName,
                        style = MaterialTheme.typography.headlineLarge,
                        textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(8.dp))
                Text(
                        text= "chce się połączyć z tym telefonem, żeby przesyłać zdjęcia.",
                        style = MaterialTheme.typography.bodyLarge,
                        textAlign = TextAlign.Center
                )
        }
}



