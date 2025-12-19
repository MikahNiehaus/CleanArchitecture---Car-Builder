import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Login';
import Register from './pages/Register';
import Cars from './pages/Cars';
import CreateCar from './pages/CreateCar';
import EditCar from './pages/EditCar';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <Layout>
                    <Cars />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/cars/create"
              element={
                <ProtectedRoute>
                  <Layout>
                    <CreateCar />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/cars/edit/:id"
              element={
                <ProtectedRoute>
                  <Layout>
                    <EditCar />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
