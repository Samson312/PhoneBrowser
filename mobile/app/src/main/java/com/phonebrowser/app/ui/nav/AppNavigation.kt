package com.phonebrowser.app.ui.nav

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.phonebrowser.app.services.pairing.PairingManager
import com.phonebrowser.app.ui.home.HomeScreen
import com.phonebrowser.app.ui.pairing.PairingScreen

@Composable
fun AppNavigation(
    modifier: Modifier = Modifier,
    navController: NavHostController = rememberNavController(),
)
{
    val incomingRequest by PairingManager.incomingRequest.collectAsStateWithLifecycle()

    LaunchedEffect(incomingRequest) {
        if(incomingRequest != null){
            navController.navigate(Route.Pairing.route)
        }
    }

    NavHost(
        navController = navController,
        startDestination = Route.Home.route,
        modifier = modifier,
    ){
        composable(Route.Home.route) { HomeScreen() }

        composable(Route.Pairing.route) {
            PairingScreen(onHandled = { navController.popBackStack() })
        }
    }
}