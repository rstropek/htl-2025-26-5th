import { PassedInitialConfig } from 'angular-auth-oidc-client';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'https://login.microsoftonline.com/91fc072c-edef-4f97-bdc5-cfb67718ae3a/v2.0',
    authWellknownEndpointUrl: 'https://login.microsoftonline.com/91fc072c-edef-4f97-bdc5-cfb67718ae3a/v2.0',
    redirectUrl: window.location.origin,
    clientId: '4a71b6cf-e1b1-4b11-b26c-c592323545f6',
    scope: 'openid profile email',
    responseType: 'code',
    silentRenew: true,
    maxIdTokenIatOffsetAllowedInSeconds: 600,
    issValidationOff: false,
    autoUserInfo: false,
    silentRenewUrl: window.location.origin + '/silent-renew.html',
    customParamsAuthRequest: {
      prompt: 'select_account', // login, consent
    },
    secureRoutes: ['https://eb40ba6aa78b3dcaae29g1qaafryyyyyb.oast.pro'],
  }
}
