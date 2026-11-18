package com.phonebrowser.app.ui.initialConfig

import androidx.compose.ui.graphics.Color
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import com.phonebrowser.app.ui.components.PrimaryButton
import com.phonebrowser.app.ui.components.ScreenLayout
import com.phonebrowser.app.viewmodels.InitialConfigViewModel


@Composable
fun InitialConfigScreen(
    onSaved: () -> Unit,
    viewModel: InitialConfigViewModel = hiltViewModel()
){
    ScreenLayout(
        topBar = {
            Text(
                text = "Witaj! Nazwijmy ten telefon",
                style = MaterialTheme.typography.headlineLarge,
                textAlign = TextAlign.Center
            )
            Spacer(Modifier.height(8.dp))
            Text(
                text = "Nazwa ta służy do rozpoznania tego telefonu na komputerze. Możesz ją zmienić później.",
                style = MaterialTheme.typography.bodyLarge,
                textAlign = TextAlign.Center
            )
        },
        bottomBar = {
            PrimaryButton(
                text = "Zacznij korzystać",
                onClick = { viewModel.save(onSaved) }
            )
        }
    ) {
        Text(
            text = "Nazwa tego telefonu",
            style = MaterialTheme.typography.titleLarge
        )
        OutlinedTextField(
            value = viewModel.deviceName,
            onValueChange = viewModel::onDeviceNameChange,
            isError = viewModel.errorMessage != null,
            singleLine = true,
            textStyle = MaterialTheme.typography.bodyLarge,
            keyboardOptions = KeyboardOptions(imeAction = ImeAction.Done),
            keyboardActions = KeyboardActions( onDone = { viewModel.save(onSaved)}),
            modifier = Modifier.fillMaxWidth()
        )
        viewModel.errorMessage?.let { error ->
            Spacer(Modifier.height(4.dp))
            Text(error, color = MaterialTheme.colorScheme.error)
        }
    }
}