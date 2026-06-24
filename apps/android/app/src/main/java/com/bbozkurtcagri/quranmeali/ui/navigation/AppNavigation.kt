package com.bbozkurtcagri.quranmeali.ui.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.outlined.Book
import androidx.compose.material.icons.outlined.Info
import androidx.compose.material.icons.outlined.Search
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.NavigationBarItemDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.bbozkurtcagri.quranmeali.ui.about.AboutScreen
import com.bbozkurtcagri.quranmeali.ui.about.AppearancePreference
import com.bbozkurtcagri.quranmeali.ui.search.SearchScreen
import com.bbozkurtcagri.quranmeali.ui.surahdetail.SurahDetailScreen
import com.bbozkurtcagri.quranmeali.ui.surahlist.SurahListScreen
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors

// Route sabitleri — tek bir NavHost altında üç tab + detay.
private object Routes {
    const val LIST = "list"
    const val SEARCH = "search"
    const val ABOUT = "about"
    // detail/{number}?verse={verse} — verse opsiyonel; yoksa başa scroll.
    const val DETAIL_PATTERN = "detail/{number}?verse={verse}"
    fun detail(surah: Int, verse: Int? = null) =
        if (verse == null) "detail/$surah" else "detail/$surah?verse=$verse"
}

private data class TabItem(val route: String, val label: String, val icon: androidx.compose.ui.graphics.vector.ImageVector)

@Composable
fun AppNavigation(
    appearance: AppearancePreference,
    onAppearanceChange: (AppearancePreference) -> Unit
) {
    val navController = rememberNavController()
    val backStack by navController.currentBackStackEntryAsState()
    val currentRoute = backStack?.destination?.route

    val tabs = listOf(
        TabItem(Routes.LIST,   "Sureler",  Icons.Outlined.Book),
        TabItem(Routes.SEARCH, "Arama",    Icons.Outlined.Search),
        TabItem(Routes.ABOUT,  "Hakkında", Icons.Outlined.Info),
    )
    val isTabRoot = currentRoute in tabs.map { it.route }

    // refreshKey — list tab'a geri dönünce chip yeniden okunsun diye.
    var listRefreshKey by remember { mutableStateOf(0) }

    Scaffold(
        bottomBar = {
            if (isTabRoot) {
                NavigationBar(
                    containerColor = MaterialTheme.qcColors.background
                ) {
                    tabs.forEach { tab ->
                        NavigationBarItem(
                            selected = currentRoute == tab.route,
                            onClick = {
                                if (currentRoute == tab.route) return@NavigationBarItem
                                if (tab.route == Routes.LIST) listRefreshKey += 1
                                navController.navigate(tab.route) {
                                    popUpTo(navController.graph.findStartDestination().id) { saveState = true }
                                    launchSingleTop = true
                                    restoreState = true
                                }
                            },
                            icon = { Icon(tab.icon, contentDescription = tab.label) },
                            label = { Text(tab.label) },
                            colors = NavigationBarItemDefaults.colors(
                                selectedIconColor = MaterialTheme.qcColors.accent,
                                selectedTextColor = MaterialTheme.qcColors.accent,
                                indicatorColor = MaterialTheme.qcColors.accentSoft,
                                unselectedIconColor = MaterialTheme.qcColors.textMuted,
                                unselectedTextColor = MaterialTheme.qcColors.textMuted
                            )
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = Routes.LIST,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(Routes.LIST) {
                SurahListScreen(
                    onSurahClick = { navController.navigate(Routes.detail(it.number)) },
                    onResume = { last ->
                        navController.navigate(Routes.detail(last.surahNumber, last.verseNumber))
                    },
                    refreshKey = listRefreshKey
                )
            }

            composable(Routes.SEARCH) {
                SearchScreen(
                    onHitClick = { hit ->
                        navController.navigate(Routes.detail(hit.surahNumber, hit.verseNumber))
                    }
                )
            }

            composable(Routes.ABOUT) {
                AboutScreen(
                    appearance = appearance,
                    onAppearanceChange = onAppearanceChange
                )
            }

            composable(
                route = Routes.DETAIL_PATTERN,
                arguments = listOf(
                    navArgument("number") { type = NavType.IntType },
                    navArgument("verse") {
                        type = NavType.StringType
                        nullable = true
                        defaultValue = null
                    }
                )
            ) { entry ->
                val number = entry.arguments?.getInt("number") ?: return@composable
                val verse = entry.arguments?.getString("verse")?.toIntOrNull()
                SurahDetailScreen(
                    surahNumber = number,
                    targetVerse = verse,
                    onBack = {
                        // List tab'ı yeniden açtığında chip taze gelsin diye.
                        listRefreshKey += 1
                        navController.popBackStack()
                    }
                )
            }
        }
    }
}
