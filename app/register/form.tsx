import * as React from 'react';
import {
  Box,
  Button,
  TextField
} from '@mui/material';

import { register } from '../utils/login';

export function RegisterForm() {
  // Component
  return (
    <Box component={"form"} action={register} className='flex flex-col w-64 space-y-4 items-center content-stretch'>
      <div className='mb-2'>
        <h1 className='text-2xl'>Register</h1>
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

      <TextField
        name="fullname"
        id='fullname'
        label='Full Name'
        type='text'
        required={true}
        variant='outlined'>
      </TextField>

      <TextField
        name="yearborn"
        id="yearborn"
        label="Year Born"
        type="number"
        required={true}
        variant='outlined'>
      </TextField>

      <Button className='w-3/5' type="submit" variant='outlined'>Log In</Button>
    </Box>
  )
}