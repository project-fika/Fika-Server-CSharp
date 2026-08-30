import { CodeHighlightAdapterProvider, createHighlightJsAdapter } from '@mantine/code-highlight';
import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import hljs from 'highlight.js/lib/core';
import jsonLang from 'highlight.js/lib/languages/json';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import 'highlight.js/styles/tokyo-night-dark.css';

import { ProtectedRoute } from './components/ProtectedRoute';
import { AuthProvider, useAuth } from './context/AuthContext';
import { AboutPage } from './pages/AboutPage';
import { AccountsPage } from './pages/AccountsPage';
import { DashboardOverviewPage } from './pages/DashboardOverviewPage';
import { DashboardPage } from './pages/DashboardPage';
import { HeadlessPage } from './pages/HeadlessPage';
import { LoginPage } from './pages/LoginPage';
import { PlayersPage } from './pages/PlayersPage';
import { ProfilesPage } from './pages/ProfilesPage';
import { StatisticsPage } from './pages/StatisticsPage';

import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import '@mantine/charts/styles.css';
import '@mantine/code-highlight/styles.css';
import { FileManagerPage } from './pages/FileManagerPage';
import { ToolsPage } from './pages/ToolsPage';
import { fikaTheme } from './theme/theme';

import 'react18-json-view/src/style.css';
import 'react18-json-view/src/dark.css';

hljs.registerLanguage('json', jsonLang);
const highlightJsAdapter = createHighlightJsAdapter(hljs);

const queryClient = new QueryClient();

function RootRedirect() {
    const { isAuthenticated } = useAuth();
    return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />;
}

export default function App() {
    return (
        <MantineProvider theme={fikaTheme} defaultColorScheme="dark">
            <CodeHighlightAdapterProvider adapter={highlightJsAdapter}>
                <Notifications position="top-right" />
                <QueryClientProvider client={queryClient}>
                    <AuthProvider>
                        <BrowserRouter>
                            <Routes>
                                <Route path="/" element={<RootRedirect />} />
                                <Route path="/login" element={<LoginPage />} />

                                {/* Authenticated Base Routes */}
                                <Route element={<ProtectedRoute />}>
                                    <Route element={<DashboardPage />}>
                                        <Route index element={<Navigate to="/dashboard" replace />} />
                                        <Route path="dashboard" element={<DashboardOverviewPage />} />
                                        <Route path="about" element={<AboutPage />} />
                                        <Route path="fika/manage/headless" element={<HeadlessPage />} />
                                        <Route path="fika/manage/players" element={<PlayersPage />} />
                                        <Route path="fika/manage/statistics" element={<StatisticsPage />} />
                                        <Route path="fika/manage/filemanager" element={<FileManagerPage />} />

                                        {/* Admin-Only Routes */}
                                        <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                                            <Route path="fika/manage/accounts" element={<AccountsPage />} />
                                        </Route>

                                        {/* Admin & Moderator Routes */}
                                        <Route element={<ProtectedRoute allowedRoles={['Admin', 'Moderator']} />}>
                                            <Route path="fika/manage/profiles" element={<ProfilesPage />} />
                                            <Route path="fika/manage/tools" element={<ToolsPage />} />
                                        </Route>
                                    </Route>
                                </Route>

                                <Route path="*" element={<Navigate to="/" replace />} />
                            </Routes>
                        </BrowserRouter>
                    </AuthProvider>
                </QueryClientProvider>
            </CodeHighlightAdapterProvider>
        </MantineProvider>
    );
}
