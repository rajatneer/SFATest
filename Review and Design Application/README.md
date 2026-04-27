
  # Review and Design Application

  This is a code bundle for Review and Design Application. The original project is available at https://www.figma.com/design/gb6hVDZtYx851JRsH8agFh/Review-and-Design-Application.

  ## Running the code

  Run `npm i` to install the dependencies.

  Create environment file for API integration:

  ```powershell
  Copy-Item .env.example .env
  ```

  Run `npm run dev` to start the development server.

  ## PWA + Offline Queue

  This app now includes:
  - Service worker registration (`public/sw.js`)
  - Web manifest (`public/manifest.webmanifest`)
  - IndexedDB sync queue powered by Dexie
  - Sync push to REST API endpoint `/api/mobile/sync/push`
  