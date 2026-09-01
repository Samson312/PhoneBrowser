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
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text(text = viewModel.statusText, style = MaterialTheme.typography.headlineSmall)
        Spacer(modifier = Modifier.height(16.dp))
        Row {
            Button(onClick = { viewModel.startDiscovery() }) { Text("Start") }
            Spacer(modifier = Modifier.width(8.dp))
            Button(onClick = { viewModel.stopDiscovery() }) { Text("Stop") }
        }
        Spacer(modifier = Modifier.height(16.dp))
        LazyColumn {
            items(viewModel.logEntries) { entry ->
                Text(entry, style = MaterialTheme.typography.bodySmall)
            }
        }
    }
}