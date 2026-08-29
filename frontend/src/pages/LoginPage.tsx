import { Alert, Button, Container, Paper, PasswordInput, TextInput, Title } from '@mantine/core';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';

export function LoginPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const { login, isAuthenticated } = useAuth();
    const navigate = useNavigate();

    // redirect to dashboard if user is already logged in
    if (isAuthenticated) {
        return <Navigate to="/dashboard" replace />;
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await api.post<{ token: string; username: string }>('/auth/login', {
                username,
                password,
            });

            login(response.data.token, response.data.username);
            navigate('/dashboard', { replace: true });
        } catch (err: unknown) {
            const message =
                err instanceof AxiosError ? err.response?.data?.message || 'Invalid username or password' : 'Invalid username or password';
            setError(message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container size={420} my={40}>
            <Title ta="center">FikaWebApp Login</Title>
            <Paper withBorder shadow="md" p={30} mt={30} radius="md">
                {error && (
                    <Alert color="red" mb="md">
                        {error}
                    </Alert>
                )}
                <form onSubmit={handleSubmit}>
                    <TextInput label="Username" required value={username} onChange={(e) => setUsername(e.target.value)} />
                    <PasswordInput label="Password" required mt="md" value={password} onChange={(e) => setPassword(e.target.value)} />
                    <Button fullWidth mt="xl" type="submit" loading={loading}>
                        Sign in
                    </Button>
                </form>
            </Paper>
        </Container>
    );
}
