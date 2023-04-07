using ClientiLibrary;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
//riferimenti database
using MySql.Data.MySqlClient;
using System.Configuration;


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
            // Crea una nuova lista vuota per memorizzare i clienti trovati
            List<Cliente> clientiTrovati = new List<Cliente>();

            try
            {
                // Utilizza un blocco using per gestire la connessione al database
                using (MySqlConnection connection = new MySqlConnection(_connectionDB))
                {
                    // Apre la connessione al database
                    connection.Open();

                    // Prepara la query SQL per cercare il cliente in base alla scelta dell'utente
                    string query = $"SELECT * FROM Clienti WHERE {scelta} = @parametroRicerca";

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
            }
            catch (InvalidOperationException ex)
            {
                // Lancia un'eccezione InvalidOperationException con un messaggio personalizzato
                throw new InvalidOperationException("Errore durante la ricerca dei clienti.", ex);
            }

            // Restituisce la lista dei clienti trovati
            return clientiTrovati;
        }

        public void AggiungiCliente(Cliente nuovoCliente)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionDB))
            {
                connection.Open();

                // Query per cercare un cliente con lo stesso ID
                string query = "SELECT COUNT(*) FROM Clienti WHERE ID = @ID";

                // Dichiarazione di un comando con la query e la connessione al database
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Impostazione del valore del parametro "@ID" nel comando
                    command.Parameters.AddWithValue("@ID", nuovoCliente.ID);

                    // Uso ToInt32 perchè ExecuteScalar() fa partire il command e restituisce un oggetto, io ho bisogno di un numero per il conteggio - count conterrà il risutato del "COUNT"
                    // (ExecuteScalar() prende il primo valore della prima riga del set di risultati, ExecuteReader() se avessi avuto più di un risultato) la query restituisce solo un valore poiché uso la funzione di aggregazione "COUNT" (conteggia le righe che soddisfano la condizione)
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    if (count > 0) // Controllo se esiste già un cliente con lo stesso ID nel database
                    {
                        // Se esiste, lancio un'eccezione con un messaggio di errore
                        throw new InvalidOperationException("L'ID del cliente esiste già nel database.");
                    }
                }

                // Dichiarazione della query per inserire il nuovo cliente
                string insertQuery = "INSERT INTO Clienti (ID, Nome, Cognome, Citta, Sesso, DataDiNascita) " + "VALUES (@ID, @Nome, @Cognome, @Citta, @Sesso, @DataDiNascita)";

                // Dichiarazione di un comando con la query e la connessione al database
                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                {
                    // Impostazione dei valori dei parametri nel comando
                    insertCommand.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = nuovoCliente.ID; ;
                    insertCommand.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = nuovoCliente.Nome;
                    insertCommand.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = nuovoCliente.Cognome;
                    insertCommand.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = nuovoCliente.Citta;
                    insertCommand.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = nuovoCliente.Sesso;
                    insertCommand.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = nuovoCliente.DataDiNascita;

                    // Esecuzione della query di inserimento e agginta del valore a rowsAffected
                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    // Controllo quante righe ha inserito la query, deve essere 1
                    if (rowsAffected != 1)
                    {
                        throw new InvalidOperationException("Errore durante l'inserimento del nuovo cliente.");
                    }
                }
            }
        }

        // MODIFICA //
        public void ModificaCliente(string id, Cliente clienteModificato) //in input i dati da modificare (clienteModificato)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionDB))
                {
                    conn.Open();

                    MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Clienti WHERE ID = @ID", conn);
                    checkCmd.Parameters.AddWithValue("@ID", id);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        throw new InvalidOperationException("Il cliente con l'ID specificato non esiste nel database.");
                    }

                    MySqlCommand cmd = new MySqlCommand("UPDATE Clienti SET Nome = @Nome, Cognome = @Cognome, Citta = @Citta, Sesso = @Sesso, DataDiNascita = @DataDiNascita WHERE ID = @ID", conn);

                    cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id;
                    cmd.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = clienteModificato.Nome;
                    cmd.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = clienteModificato.Cognome;
                    cmd.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = clienteModificato.Citta;
                    cmd.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = clienteModificato.Sesso;
                    cmd.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = clienteModificato.DataDiNascita;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("Modifica del cliente non riuscita.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Si è verificato un errore sconosciuto durante la modifica del cliente.", ex);
            }
        }

        // ELIMINA //
        public bool EliminaCliente(string id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionDB))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Clienti WHERE ID = @ID", conn);
                    cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id;
                    // ExecuteNonQuery() restituisce il numero di righe interessate dalla query non dei dati, quindi lo associo a rowsAffected
                    int rowsAffected = cmd.ExecuteNonQuery();
                    // Controllo se la query ha eliminato almeno una riga
                    return rowsAffected > 0;
                }
            }
            catch (MySqlException ex)
            {
                //", ex" serve per stampare il messaggio di errore predefinito di MySqlException e capire il vero errore
                throw new InvalidOperationException("Errore durante l'eliminazione del cliente.", ex);
                //return false; // non serve più se c'è InvalidOperationException
            }
        }


        public bool VerificaIDUnivoco(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionDB))
            {
                connection.Open();

                // Selezionara tutti gli ID dal database
                string query = "SELECT ID FROM clienti";

                // Crea un oggetto MySqlCommand, passando la query e la connessione al db (procedura standard)
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Legge gli ID dal database utilizzando il metodo ExecuteReader del command (MySqlCommand)
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Controlla se l'ID cercato esiste già nella lista degli ID letti dal database
                        //Il metodo Read() sposta il cursore del lettore sulla riga successiva del risultato, ritornando true se ci sono altre righe disponibili.
                        while (reader.Read())
                        {
                            // GetString mi serve per leggere il valore della riga che cambierà di continuo grazie al while
                            if (id == reader.GetString(0)) // Lo 0 serve per indicare che deve leggere le rihe della colonna 0
                            {
                                return false; // L'ID non è univoco
                            }
                        }
                    }
                }
            }
            return true; // L'ID è univoco
        }
    }
}