package com.phonebrowser.app.ui.nav

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.hilt.lifecycle.viewmodel.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.phonebrowser.app.services.pairing.PairingManager
import com.phonebrowser.app.ui.home.HomeScreen
import com.phonebrowser.app.ui.initialConfig.InitialConfigScreen
import com.phonebrowser.app.ui.pairing.PairingScreen
import com.phonebrowser.app.viewmodels.AppViewModel

@Composable
fun AppNavigation(
    modifier: Modifier = Modifier,
    navController: NavHostController = rememberNavController(),
    appViewModel: AppViewModel = hiltViewModel()
)
{
    val incomingRequest by PairingManager.incomingRequest.collectAsStateWithLifecycle()
    val hasProfile by appViewModel.hasProfile.collectAsStateWithLifecycle()

    LaunchedEffect(incomingRequest) {
        if(incomingRequest != null){
            navController.navigate(Route.Pairing.route)
        }
    }

    if (hasProfile == null) return

    NavHost(
        navController = navController,
        startDestination = if (hasProfile == true) Route.Home.route else Route.InitialConfig.route,
        modifier = modifier,
    ){
        composable(Route.InitialConfig.route) {
            InitialConfigScreen(onSaved = {
                navController.navigate(Route.Home.route) {
                        popUpTo(Route.InitialConfig.route) { inclusive = true }
                    }
                }
            )
        }

        composable(Route.Home.route) { HomeScreen() }

        composable(Route.Pairing.route) {
            PairingScreen(onHandled = { navController.popBackStack() })
        }
    }
}