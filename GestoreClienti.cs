using ClientiLibrary;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
//riferimenti database
using MySql.Data.MySqlClient;
using System.Collections;
using MySqlX.XDevAPI;

namespace AssemblyGestore
{
    public class GestoreClienti : IGestoreC
    {
        private string _connectionDB;

        // Costruttore che accetta il percorso come argomento
        public GestoreClienti(string connectionDB)
        {
            _connectionDB = connectionDB;
        }

        // CERCA //
        public List<Cliente> CercaCliente(string parametroRicerca, string scelta)
        {
            List<Cliente> clientiTrovati = new List<Cliente>();  // Crea una nuova lista vuota per memorizzare i clienti trovati

            // Verifica che il parametro di ricerca non sia nullo o vuoto
            if (string.IsNullOrEmpty(parametroRicerca))
            {
                throw new ArgumentException("Il parametro di ricerca non può essere vuoto.");
            }

            var tipiRicercaValidi = new HashSet<string> { "ID", "Nome", "Cognome", "Citta", "Sesso", "DataDiNascita" };
            if (!tipiRicercaValidi.Contains(scelta))
            {
                throw new ArgumentException("Il tipo di ricerca non è valido.", nameof(scelta));
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionDB))
                {
                    connection.Open();

                    string query = $"SELECT * FROM Clienti WHERE {scelta} = @parametroRicerca"; // Query SQL per cercare il cliente in base alla scelta dell'utente

                    // Crea un nuovo comando MySQL con la query e la connessione al database
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Imposta il valore del parametro nel comando
                        command.Parameters.AddWithValue("@parametroRicerca", parametroRicerca);

                        // Esegui la query e ottieni i risultati nell'oggetto MySqlDataReader 'reader'
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Leggi i risultati riga per riga
                            while (reader.Read())
                            {

                                // Crea un nuovo oggetto Cliente dai dati letti
                                Cliente cliente = new Cliente(
                                    reader.GetString("ID"),
                                    reader.GetString("Nome"),
                                    reader.GetString("Cognome"),
                                    reader.GetString("Citta"),
                                    reader.GetString("Sesso"),
                                    reader.GetDateTime("DataDiNascita"));

                                // Aggiungi il cliente trovato alla lista dei clienti trovati
                                clientiTrovati.Add(cliente);
                            }
                        }
                    }
                }
                // Controlla se la lista dei clienti trovati è vuota // E' UN ECCEZIONE SPECIFICA, QUINDI LA METTO FUORI DAL BLOCCO TRY-CATCH,
                if (clientiTrovati.Count == 0)
                {
                    throw new InvalidOperationException("Nessun cliente trovato con il parametro di ricerca specificato.");
                }
            }
            catch (MySqlException ex)
            {
                // ex.Message restituisce solo il messaggio di errore dell'eccezione, mentre ex restituisce l'intera eccezione, compresi i dettagli
                throw new InvalidOperationException("Errore durante connessine al  database. Messaggio di errore: " + ex);
            }
            catch (Exception ex)
            {
                // Lancia un'eccezione con un messaggio personalizzato per tutti gli altri errori
                throw new InvalidOperationException(ex.Message);
            
            }
            // Restituisce la lista dei clienti trovati
            return clientiTrovati;
        }

        public void AggiungiCliente(Cliente cliente)
        {
            Cliente.ValidaId(cliente.ID);
            Cliente.ValidaSesso(cliente.Sesso);
            Cliente.ValidaData(cliente.DataDiNascita.ToString("dd/MM/yyyy"));
            Cliente.ValidaInput(cliente.Nome);
            Cliente.ValidaInput(cliente.Cognome);
            Cliente.ValidaInput(cliente.Citta);
            VerificaIdUnivocoDB("Clienti", "ID", cliente.ID);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionDB))
                {
                    conn.Open();


                    string query = "INSERT INTO Clienti (ID, Nome, Cognome, Citta, Sesso, DataDiNascita) VALUES (@ID, @Nome, @Cognome, @Citta, @Sesso, @DataDiNascita)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = cliente.ID;
                        cmd.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = cliente.Nome;
                        cmd.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = cliente.Cognome;
                        cmd.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = cliente.Citta;
                        cmd.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = cliente.Sesso;
                        cmd.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = cliente.DataDiNascita;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        throw new InvalidOperationException("Errore durante la connessione al database.", ex);
                    case 1062:
                        throw new InvalidOperationException("Cliente già presente nel database.", ex);
                    default:
                        throw new InvalidOperationException("Errore durante l'inserimento del cliente nel database.", ex);
                }
            }
        }

        public void ModificaCliente(string id, Cliente clienteModificato) //in input i dati da modificare (clienteModificato)
        {
            Cliente.ValidaId(id);
            Cliente.ValidaSesso(clienteModificato.Sesso);
            Cliente.ValidaData(clienteModificato.DataDiNascita.ToString("dd/MM/yyyy"));
            Cliente.ValidaInput(clienteModificato.Nome);
            Cliente.ValidaInput(clienteModificato.Cognome);
            Cliente.ValidaInput(clienteModificato.Citta);
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(_connectionDB);
                conn.Open();

                VerificaIdUnivocoDB("Clienti", "ID", clienteModificato.ID);

                MySqlCommand cmd = new MySqlCommand("UPDATE Clienti SET Nome = @Nome, Cognome = @Cognome, Citta = @Citta, Sesso = @Sesso, DataDiNascita = @DataDiNascita WHERE ID = @ID", conn);

                cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id;
                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = clienteModificato.Nome;
                cmd.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = clienteModificato.Cognome;
                cmd.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = clienteModificato.Citta;
                cmd.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = clienteModificato.Sesso;
                cmd.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = clienteModificato.DataDiNascita;

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("Modifica del cliente non riuscita.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Si è verificato un errore sconosciuto durante la modifica del cliente.", ex);
            }
            // uso finally perché non ho lo "use" per la connessione del database
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        // ELIMINA //
        public bool EliminaCliente(string id)
        {
            int rowsAffected;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionDB))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Clienti WHERE ID = @ID", conn);
                    cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id;
                    // ExecuteNonQuery() restituisce il numero di righe interessate dalla query non dei dati, quindi lo associo a rowsAffected
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                //", ex" serve per stampare il messaggio di errore predefinito di MySqlException e capire il vero errore
                throw new InvalidOperationException("Errore durante l'eliminazione del cliente.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Non hai inserito un input valido.", ex);
            }

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Nessun cliente trovato con l'ID specificato.");
            }

            // Controllo se la query ha eliminato almeno una riga
            return rowsAffected > 0;
        }


        private void VerificaIdUnivocoDB(string tableName, string columnName, string id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionDB))
            {
                conn.Open();
                string query = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @ID";
                MySqlCommand checkCmd = new MySqlCommand(query, conn);
                checkCmd.Parameters.AddWithValue("@ID", id);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                // Se non esite l'id cercato il count sarà uguale a 0
                if (count > 0)
                {
                    throw new InvalidOperationException("L'elemento con l'ID specificato è già presente nel database.");
                }
            }
        }
        public void VerificaIdUnivoco(string id)
        {
            VerificaIdUnivocoDB("Clienti", "ID", id);
        }
        ////Per sicurezza il metodosopra rimane privato, questo mi sevirà nel Program.cs
        //public bool VerificaIdPubblico(string id)
        //{
        //    using (MySqlConnection conn = new MySqlConnection(_connectionDB))
        //    {
        //        conn.Open();
        //        string query = $"SELECT COUNT(*) FROM Clienti WHERE ID = @ID";
        //        MySqlCommand checkCmd = new MySqlCommand(query, conn);
        //        checkCmd.Parameters.AddWithValue("@ID", id);
        //        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

        //        return count > 0;
        //    }
        //}

    }
}

        //public bool VerificaIDUnivoco(string id)
        //{
        //    using (MySqlConnection connection = new MySqlConnection(_connectionDB))
        //    {
        //        connection.Open();

        //        // Selezionara tutti gli ID dal database
        //        string query = "SELECT ID FROM clienti";

        //        // Crea un oggetto MySqlCommand, passando la query e la connessione al db (procedura standard)
        //        using (MySqlCommand command = new MySqlCommand(query, connection))
        //        {
        //            // Legge gli ID dal database utilizzando il metodo ExecuteReader del command (MySqlCommand)
        //            using (MySqlDataReader reader = command.ExecuteReader())
        //            {
        //                // Controlla se l'ID cercato esiste già nella lista degli ID letti dal database
        //                //Il metodo Read() sposta il cursore del lettore sulla riga successiva del risultato, ritornando true se ci sono altre righe disponibili.
        //                while (reader.Read())
        //                {
        //                    // GetString mi serve per leggere il valore della riga che cambierà di continuo grazie al while
        //                    if (id == reader.GetString(0)) // Lo 0 serve per indicare che deve leggere le rihe della colonna 0
        //                    {
        //                        return false; // L'ID non è univoco
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return true; // L'ID è univoco
        //}