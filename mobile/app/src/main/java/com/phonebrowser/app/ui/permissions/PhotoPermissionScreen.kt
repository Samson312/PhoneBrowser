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
import com.phonebrowser.app.ui.components.PrimaryButton
import com.phonebrowser.app.ui.components.ScreenLayout
import com.phonebrowser.app.viewmodels.PhotoPermissionViewModel

@Composable
fun PhotoPermissionScreen(
    onGranted: () -> Unit,
    viewModel: PhotoPermissionViewModel = hiltViewModel()
){
    val context = LocalContext.current

    val launcher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted -> viewModel.onPermissionResult(granted) }

    LifecycleEventEffect(Lifecycle.Event.ON_RESUME) { viewModel.refresh() }

    LaunchedEffect(viewModel.isGranted){
        if(viewModel.isGranted) onGranted()
    }

    val permissionDescription = when {
        Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE -> {
            "Po kliknięciu „Zezwól na dostęp do zdjęć” wybierz „Zezwól na wszystkie”, aby umożliwić przesyłanie zdjęć."
        }
        else -> {
            "Po kliknięciu „Zezwól na dostęp do zdjęć” wybierz „Zezwól”, aby umożliwić przesyłanie zdjęć."
        }
    }

    val denied = viewModel.isDenied

    ScreenLayout(
        bottomBar = {
            PrimaryButton(
                text = if (denied) "Otwórz ustawienia"
                else "Zezwól na dostęp do zdjęć",
                onClick = {
                    if (denied) {
                        context.startActivity(
                            Intent(
                                Settings.ACTION_APPLICATION_DETAILS_SETTINGS,
                                Uri.fromParts("package", context.packageName, null)
                            )
                        )
                    } else {
                        launcher.launch(viewModel.permission)
                    }
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