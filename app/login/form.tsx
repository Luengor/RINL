import * as React from 'react';
import {
  Alert,
  Box,
  Button,
  TextField
} from '@mui/material';

import { login } from '../utils/login';
import { useSearchParams } from 'next/navigation'    

export function LoginForm() {
  // Check if it failed
  const searchParams = useSearchParams();
  let failed = false

  if (searchParams.get('failed'))
    failed = true;
  
  // Component
  return (
    <Box component={"form"} action={login} className='flex flex-col w-64 space-y-4 items-center content-stretch'>
      {failed && <Alert severity="error">Login failed</Alert>}

      <div className='mb-2'>
        <h1 className='text-2xl'>Log In</h1>
      </div>

      <TextField
        name="username"
        id="username"
        label="Username"
        type="email"
        required={true}
        variant='outlined'>
      </TextField>

      <TextField
        name="password"
        id="password"
        label="Password"
        type="password"
        required={true}
        variant='outlined'>
      </TextField>

      <Button className='w-3/5' type="submit" variant='outlined'>Log In</Button>
    </Box>
  )
}