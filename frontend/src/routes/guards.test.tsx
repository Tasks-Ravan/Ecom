import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { AdminRoute } from './guards';
import { AuthContext, type AuthContextValue } from '../lib/auth-context';

const session = (roles: ('Admin' | 'Customer')[], authenticated = true): AuthContextValue => ({
  user: authenticated ? { id: 'user-1', email: 'user@example.com', roles } : null,
  isReady: true,
  isAuthenticated: authenticated,
  isAdmin: roles.includes('Admin'),
  signIn: async () => ({ id: 'user-1', email: 'user@example.com', roles }),
  completeSignIn: () => ({ id: 'user-1', email: 'user@example.com', roles }),
  signOut: () => undefined,
});

describe('AdminRoute', () => {
  it('redirects a customer to the forbidden page', () => {
    render(<AuthContext.Provider value={session(['Customer'])}><MemoryRouter initialEntries={['/admin']}><Routes>
      <Route element={<AdminRoute />}><Route path="/admin" element={<div>Admin content</div>} /></Route>
      <Route path="/403" element={<div>Forbidden screen</div>} />
    </Routes></MemoryRouter></AuthContext.Provider>);
    expect(screen.getByText('Forbidden screen')).toBeInTheDocument();
  });

  it('allows an Admin through', () => {
    render(<AuthContext.Provider value={session(['Admin'])}><MemoryRouter initialEntries={['/admin']}><Routes>
      <Route element={<AdminRoute />}><Route path="/admin" element={<div>Admin content</div>} /></Route>
    </Routes></MemoryRouter></AuthContext.Provider>);
    expect(screen.getByText('Admin content')).toBeInTheDocument();
  });
});
