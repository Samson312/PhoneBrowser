package com.phonebrowser.app.ui.initialConfig

import androidx.compose.ui.graphics.Color
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import com.phonebrowser.app.viewmodels.InitialConfigViewModel

@Composable
fun InitialConfigScreen(
    onSaved: () -> Unit,
    viewModel: InitialConfigViewModel = hiltViewModel()
){
    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Spacer(Modifier.height(64.dp))
        Text(
            text = "Skonfigurujmy aplikację",
            fontSize = 28.sp
        )
        Spacer(Modifier.height(8.dp))
        Text(
            text = "To można zmienić później w ustawieniach.",
            textAlign = TextAlign.Center,
            fontSize = 18.sp
        )

        Column(
            modifier = Modifier.weight(1f).fillMaxWidth(),
            verticalArrangement = Arrangement.Center
        ) {
            Text(
                text = "Nazwa tego telefonu",
                fontSize = 24.sp
            )
            Spacer(Modifier.height(6.dp))
            OutlinedTextField(
                value = viewModel.deviceName,
                onValueChange = viewModel::onDeviceNameChange,
                isError = viewModel.errorMessage != null,
                colors = OutlinedTextFieldDefaults.colors(
                    focusedTextColor = Color.Black,
                    unfocusedTextColor = Color.Black,
                ),
                modifier = Modifier.fillMaxWidth()
            )
            viewModel.errorMessage?.let { error ->
                Spacer(Modifier.height(4.dp))
                Text(error, color = MaterialTheme.colorScheme.error)
            }
        }

        Button(
            onClick = { viewModel.save(onSaved) }) {
            Text(
                text = "Zacznij korzystać",
                fontSize = 28.sp
            )
        }

    }
}