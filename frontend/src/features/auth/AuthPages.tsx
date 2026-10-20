import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowRight, Eye, EyeOff, LockKeyhole, Mail } from 'lucide-react';
import { useEffect, useRef, useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { register as registerAccount } from '../../api/auth';
import { apiBaseUrl } from '../../api/client';
import { exchangeGoogleLoginCode } from '../../api/auth';
import { ApiError } from '../../api/client';
import type { LoginFormValues, RegisterFormValues } from '../../lib/schemas';
import { loginSchema, registerSchema } from '../../lib/schemas';
import { useAuth } from '../../lib/auth-context';
import { useTranslation } from 'react-i18next';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';

export function LoginPage() {
  const { t } = useTranslation();
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });
  const submit = handleSubmit(async (values) => {
    setServerError('');
    try {
      const signedInUser = await signIn(values.email, values.password);
      const destination = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname;
      navigate(destination || (signedInUser.roles.includes('Admin') ? '/admin' : '/'), { replace: true });
    } catch (error) {
      setServerError(error instanceof ApiError && error.status !== 401 ? error.message : t('auth.invalid'));
    }
  });
  return <section className="auth-card">
    <div className="auth-icon"><LockKeyhole size={22} /></div><h1>{t('auth.signInTitle')}</h1><p>{t('auth.signInBody')}</p>
    <form className="form-stack" onSubmit={(event) => void submit(event)} noValidate>
      <FormField label={t('auth.email')} error={errors.email?.message} icon={<Mail size={16} />}>
        <Input type="email" autoComplete="email" {...register('email')} />
      </FormField>
      <FormField label={t('auth.password')} error={errors.password?.message} icon={<LockKeyhole size={16} />}>
        <div className="password-input"><Input type={showPassword ? 'text' : 'password'} autoComplete="current-password" {...register('password')} />
          <button type="button" className="password-toggle" onClick={() => setShowPassword((value) => !value)} aria-label={t('auth.togglePassword')}>
            {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}</button></div>
      </FormField>
      {serverError && <p className="form-error" role="alert">{serverError}</p>}
      <Button type="submit" className="w-full" disabled={isSubmitting}>{isSubmitting ? t('common.loading') : t('auth.signIn')} <ArrowRight size={16} /></Button>
    </form>
    <div className="my-4 text-center text-sm muted">— {t('auth.or')} —</div>
    <Button type="button" variant="secondary" className="w-full" onClick={() => window.location.assign(`${apiBaseUrl}/auth/google`)}>
      {t('auth.continueWithGoogle')}
    </Button>
    <p className="auth-footer">{t('auth.noAccount')} <Link to="/register">{t('auth.signUp')}</Link></p>
  </section>;
}

export function GoogleOAuthCallbackPage() {
  const { t } = useTranslation();
  const { completeSignIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [error, setError] = useState('');
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;
    const params = new URLSearchParams(location.search);
    const code = params.get('code');
    window.history.replaceState({}, document.title, location.pathname);
    if (!code || params.has('error')) {
      setError(t(params.get('error') === 'google_not_configured' ? 'auth.googleNotConfigured' : 'auth.googleFailed'));
      return;
    }

    void exchangeGoogleLoginCode(code)
      .then((session) => {
        const user = completeSignIn(session);
        navigate(user.roles.includes('Admin') ? '/admin' : '/', { replace: true });
      })
      .catch(() => setError(t('auth.googleFailed')));
  }, [completeSignIn, location.pathname, location.search, navigate, t]);

  return <section className="auth-card">
    <div className="auth-icon"><LockKeyhole size={22} /></div>
    <h1>{error ? t('auth.googleFailedTitle') : t('common.loading')}</h1>
    <p role={error ? 'alert' : undefined}>{error || t('auth.googleCompleting')}</p>
    {error && <p className="auth-footer"><Link to="/login">{t('auth.signIn')}</Link></p>}
  </section>;
}

export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });
  const submit = handleSubmit(async (values) => {
    setServerError('');
    try {
      await registerAccount(values);
      toast.success(t('auth.accountCreated'));
      navigate('/login', { replace: true });
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : t('errors.request'));
    }
  });
  return <section className="auth-card">
    <div className="auth-icon"><LockKeyhole size={22} /></div><h1>{t('auth.signUpTitle')}</h1><p>{t('auth.signUpBody')}</p>
    <form className="form-stack" onSubmit={(event) => void submit(event)} noValidate>
      <div className="grid grid-cols-2 gap-4">
        <FormField label={t('auth.firstName')} error={errors.firstName?.message}><Input autoComplete="given-name" {...register('firstName')} /></FormField>
        <FormField label={t('auth.lastName')} error={errors.lastName?.message}><Input autoComplete="family-name" {...register('lastName')} /></FormField>
      </div>
      <FormField label={t('auth.email')} error={errors.email?.message}><Input type="email" autoComplete="email" {...register('email')} /></FormField>
      <FormField label={t('auth.password')} error={errors.password?.message}>
        <div className="password-input">
          <Input type={showPassword ? 'text' : 'password'} autoComplete="new-password" {...register('password')} />
          <button type="button" className="password-toggle" onClick={() => setShowPassword((value) => !value)} aria-label={t('auth.togglePassword')} aria-pressed={showPassword}>
            {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
          </button>
        </div>
      </FormField>
      <p className="muted text-xs leading-relaxed">{t('auth.passwordRules')}</p>
      {serverError && <p className="form-error" role="alert">{serverError}</p>}
      <Button type="submit" className="w-full" disabled={isSubmitting}>{isSubmitting ? t('common.loading') : t('auth.signUp')} <ArrowRight size={16} /></Button>
    </form>
    <p className="auth-footer">{t('auth.haveAccount')} <Link to="/login">{t('auth.signIn')}</Link></p>
  </section>;
}

function FormField({ label, error, icon, children }: { label: string; error?: string; icon?: ReactNode; children: ReactNode }) {
  return <label className="form-field"><span className="field-label">{label}</span><span className={icon ? 'field-with-icon' : ''}>{icon && <span className="field-icon">{icon}</span>}{children}</span>
    {error && <span className="form-error">{error}</span>}</label>;
}
