package com.phonebrowser.app.ui.permissions

import android.os.Build
import android.content.Intent
import android.net.Uri
import android.provider.Settings
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.compose.ui.Alignment
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.sp
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.compose.LifecycleEventEffect
import com.phonebrowser.app.services.permissions.PhotoAccess
import com.phonebrowser.app.ui.components.PrimaryButton
import com.phonebrowser.app.ui.components.ScreenLayout
import com.phonebrowser.app.ui.components.SecondaryButton
import com.phonebrowser.app.viewmodels.PhotoPermissionViewModel

@Composable
fun PhotoPermissionScreen(
    onGranted: () -> Unit,
    viewModel: PhotoPermissionViewModel = hiltViewModel()
){
    val context = LocalContext.current

    val launcher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestMultiplePermissions()
    ) { viewModel.onPermissionResult() }

    LifecycleEventEffect(Lifecycle.Event.ON_RESUME) { viewModel.refresh() }

    val state = viewModel.state

    LaunchedEffect(state?.access) {
        if (state?.access == PhotoAccess.FULL) onGranted()
    }

    val current = state ?: return
    if (current.access == PhotoAccess.FULL) return

    val openSettings = {
        context.startActivity(
            Intent(
                Settings.ACTION_APPLICATION_DETAILS_SETTINGS,
                Uri.fromParts("package", context.packageName, null)
            )
        )
    }

    when {
        current.access == PhotoAccess.PARTIAL && current.photoCount > 0 -> {
            ScreenLayout(
                bottomBar = {
                    PrimaryButton(
                        text = "Kontynuuj z wybranymi zdjęciami",
                        onClick = onGranted
                    )
                    Spacer(Modifier.height(12.dp))
                    SecondaryButton(
                        text = "Zmień wybór zdjęć",
                        onClick = { launcher.launch(viewModel.permissions) }
                    )
                    Spacer(Modifier.height(12.dp))
                    SecondaryButton(
                        text = "Zezwól na wszystkie zdjęcia",
                        onClick = openSettings
                    )
                }
            ) {
                Text(
                    text = "Wybrano ${polishPhotos(current.photoCount)}",
                    style = MaterialTheme.typography.headlineLarge,
                    textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(16.dp))
                Text(
                    text = "Na komputerze zobaczysz tylko te zdjęcia. Pozostałych zdjęć z telefonu nie będzie można przesłać.",
                    style = MaterialTheme.typography.bodyLarge,
                    textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(12.dp))
                Text(
                    text = "Aby przesyłać wszystkie zdjęcia, naciśnij „Zezwól na wszystkie zdjęcia”, wybierz „Uprawnienia”, potem „Zdjęcia” i „Zezwól na wszystkie”.",
                    style = MaterialTheme.typography.bodyLarge,
                    textAlign = TextAlign.Center
                )
            }
        }

        current.access == PhotoAccess.PARTIAL -> {
            ScreenLayout(
                bottomBar = {
                    PrimaryButton(
                        text = "Wybierz zdjęcia",
                        onClick = { launcher.launch(viewModel.permissions) }
                    )
                    Spacer(Modifier.height(12.dp))
                    SecondaryButton(
                        text = "Otwórz ustawienia",
                        onClick = openSettings
                    )
                }
            ) {
                Text(
                    text = "Nie wybrano żadnych zdjęć",
                    style = MaterialTheme.typography.headlineLarge,
                    textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(16.dp))
                Text(
                    text = "Bez wybranych zdjęć nie mamy czego przesłać. Wybierz zdjęcia albo zezwól na dostęp do wszystkich.",
                    style = MaterialTheme.typography.bodyLarge,
                    textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(12.dp))
                Text(
                    text = "Aby przesyłać wszystkie zdjęcia, naciśnij „Otwórz ustawienia”, wybierz „Uprawnienia”, potem „Zdjęcia” i „Zezwól na wszystkie”.",
                    style = MaterialTheme.typography.bodyLarge,
                    textAlign = TextAlign.Center
                )
            }
        }

        else -> {
            val denied = viewModel.isDenied
            val permissionDescription = when {
                Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE ->
                    "Po kliknięciu „Zezwól na dostęp do zdjęć” wybierz „Zezwól na wszystkie”, aby umożliwić przesyłanie zdjęć."
                else ->
                    "Po kliknięciu „Zezwól na dostęp do zdjęć” wybierz „Zezwól”, aby umożliwić przesyłanie zdjęć."
            }

            ScreenLayout(
                bottomBar = {
                    PrimaryButton(
                        text = if (denied) "Otwórz ustawienia" else "Zezwól na dostęp do zdjęć",
                        onClick = {
                            if (denied) openSettings()
                            else launcher.launch(viewModel.permissions)
                        }
                    )
                }
            ) {
                Text(
                    text = if (denied) "Aby przesyłać zdjęcia na komputer, musisz zezwolić na dostęp do zdjęć."
                    else "Zezwól na dostęp do zdjęć",
                    style = MaterialTheme.typography.headlineLarge,
                    textAlign = TextAlign.Center
                )
                Spacer(Modifier.height(16.dp))
                Text(
                    text = if (denied) "Naciśnij „Otwórz ustawienia”, wybierz „Uprawnienia”, potem „Zdjęcia” i zezwól na dostęp."
                    else permissionDescription,
                    style = MaterialTheme.typography.bodyLarge,
                    textAlign = TextAlign.Center
                )
            }
        }
    }
}

private fun polishPhotos(n: Int): String {
    if (n == 1) return "1 zdjęcie"
    val last = n % 10
    val lastTwo = n % 100
    return if (last in 2..4 && lastTwo !in 12..14) "$n zdjęcia" else "$n zdjęć"
}