using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppRazor.SeidoHelpers;
using Newtonsoft.Json;
using Services;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;
using Models;

namespace AppRazor.Pages
{
    public class EditFriendModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _friend_service = null;
        readonly IPetsService _pet_service = null;
        readonly IQuotesService _quote_service = null;
        readonly IAddressesService _address_service = null;
        readonly ILogger<EditFriendModel> _logger = null;

        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        [BindProperty]
        public FriendIM? FriendInput { get; set; }

        //I also use BindProperty to keep between several posts, bound to hidden <input> field
        [BindProperty]
        public string PageHeader { get; set; }

        //For Validation
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry>>());

        #region HTTP Requests
        public async Task<IActionResult> OnGet()
        {
            if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
            {
                //Read a friend
                var friend = await _friend_service.ReadFriendAsync(_friendId, false);

                //Populate the InputModel from the friend
                FriendInput = new FriendIM(friend.Item);
                PageHeader = "Edit details of a friend";

            }
            else
            {
                //Create an empty friend
                FriendInput = new FriendIM();
                FriendInput.StatusIM = StatusIM.Inserted;

                PageHeader = "Create a new friend";
            }

            return Page();
        }

        public IActionResult OnPostDeletePet(Guid petId)
        {
            //Find the pet to delete
            var pet = FriendInput.Pets.First(p => p.PetId == petId);
            
            //If it was just inserted (never saved to DB), remove it from the list entirely
            if (pet.StatusIM == StatusIM.Inserted)
            {
                FriendInput.Pets.Remove(pet);
            }
            else
            {
                //Otherwise mark it as deleted so it will be removed from DB
                pet.StatusIM = StatusIM.Deleted;
            }

            return Page();
        }

        public IActionResult OnPostAddPet()
        {
            string[] keys = { "FriendInput.NewPet.Name",
                              "FriendInput.NewPet.Kind"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Pet as Inserted, it will later be inserted in the database
            FriendInput.NewPet.StatusIM = StatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and editited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            FriendInput.NewPet.PetId = Guid.NewGuid();

            //Add it to the Input Models pets
            FriendInput.Pets.Add(new PetIM(FriendInput.NewPet));

            //Clear the NewPet so another pet can be added
            FriendInput.NewPet = new PetIM();

            return Page();
        }

        public IActionResult OnPostEditPet(Guid petId)
        {
            int idx = FriendInput.Pets.FindIndex(p => p.PetId == petId);
            string[] keys = { $"FriendInput.Pets[{idx}].editName",
                            $"FriendInput.Pets[{idx}].editKind"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Pet as Modified, it will later be updated in the database
            var p = FriendInput.Pets.First(pet => pet.PetId == petId);
            if (p.StatusIM != StatusIM.Inserted)
            {
                p.StatusIM = StatusIM.Modified;
            }

            //Implement the changes
            p.Name = p.editName;
            p.Kind = p.editKind;

            return Page();
        }


        public IActionResult OnPostDeleteQuote(Guid quoteId)
        {
            //Find the quote to delete
            var quote = FriendInput.Quotes.First(q => q.QuoteId == quoteId);
            
            //If it was just inserted (never saved to DB), remove it from the list entirely
            if (quote.StatusIM == StatusIM.Inserted)
            {
                FriendInput.Quotes.Remove(quote);
            }
            else
            {
                //Otherwise mark it as deleted so it will be removed from DB
                quote.StatusIM = StatusIM.Deleted;
            }

            return Page();
        }

        public IActionResult OnPostAddQuote()
        {
            string[] keys = { "FriendInput.NewQuote.QuoteText",
                              "FriendInput.NewQuote.Author"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Quote as Inserted, it will later be inserted in the database
            FriendInput.NewQuote.StatusIM = StatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and editited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            FriendInput.NewQuote.QuoteId = Guid.NewGuid();

            //Add it to the Input Models quotes
            FriendInput.Quotes.Add(new QuoteIM(FriendInput.NewQuote));

            //Clear the NewQuote so another quote can be added
            FriendInput.NewQuote = new QuoteIM();

            return Page();
        }

        public IActionResult OnPostEditQuote(Guid quoteId)
        {
            int idx = FriendInput.Quotes.FindIndex(q => q.QuoteId == quoteId);
            string[] keys = { $"FriendInput.Quotes[{idx}].editQuoteText",
                            $"FriendInput.Quotes[{idx}].editAuthor"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Quote as Modified, it will later be updated in the database
            var q = FriendInput.Quotes.First(quote => quote.QuoteId == quoteId);
            if (q.StatusIM != StatusIM.Inserted)
            {
                q.StatusIM = StatusIM.Modified;
            }

            //Implement the changes
            q.QuoteText = q.editQuoteText;
            q.Author = q.editAuthor;

            return Page();
        }

        public async Task<IActionResult> OnPostUndo()
        {
            //Reload friend from Database
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);

            //Repopulate the InputModel
            FriendInput = new FriendIM(friend.Item);
            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        {
            string[] keys = { "FriendInput.FirstName",
                              "FriendInput.LastName",
                              "FriendInput.Birthday",
                              "FriendInput.Address.StreetAddress",
                              "FriendInput.Address.ZipCode",
                              "FriendInput.Address.City",
                              "FriendInput.Address.Country"};
            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            // Save or update address first
            var addressDto = FriendInput.Address.ToCUdto();
            ResponseItemDto<IAddress> addressResp;
            if (FriendInput.Address.AddressId == Guid.Empty)
            {
                addressResp = await _address_service.CreateAddressAsync(addressDto);
            }
            else
            {
                addressDto.AddressId = FriendInput.Address.AddressId;
                addressResp = await _address_service.UpdateAddressAsync(addressDto);
            }

            // Set AddressId on FriendCuDto
            var friendDto = FriendInput.ToCUdto();
            friendDto.AddressId = addressResp.Item?.AddressId ?? Guid.Empty;

            // Check if creating new friend or updating existing one
            ResponseItemDto<IFriend> friendResp;
            if (FriendInput.StatusIM == StatusIM.Inserted)
            {
                // Create new friend
                friendResp = await _friend_service.CreateFriendAsync(friendDto);
                FriendInput.FriendId = friendResp.Item.FriendId;
            }
            else
            {
                // Update existing friend
                friendResp = await _friend_service.UpdateFriendAsync(friendDto);
            }

            // Save pets and quotes changes
            await SavePets();
            await SaveQuotes();

            return RedirectToPage("/Friends/Overview");
        }
        #endregion

        #region InputModel Pets and Quotes saved to database
        private async Task<IFriend> SavePets()
        {
            // Read the current state from database first
            var currentFriend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);
            var existingPetIds = currentFriend.Item.Pets?.Select(p => p.PetId).ToHashSet() ?? new HashSet<Guid>();

            //Check if there are deleted pets, if so simply remove them
            var deletedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedPets)
            {
                // Only try to delete if it actually exists in the database
                if (existingPetIds.Contains(item.PetId))
                {
                    //Remove from the database
                    await _pet_service.DeletePetAsync(item.PetId);
                }
            }

            //Check if there are any new pets added, if so create them in the database
            var newPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Inserted));
            foreach (var item in newPets)
            {
                //Create the corresposning model and CUdto objects
                var cuDto = item.CreateCUdto();

                //Set the relationships of a newly created item and write to database
                cuDto.FriendId = FriendInput.FriendId;
                await _pet_service.CreatePetAsync(cuDto);
            }

            //Note that now the deleted pets will be removed and created pets added. I can focus on Pet update
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);

            //Check if there are any modified pets , if so update them in the database
            var modifiedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedPets)
            {
                var model = friend.Item.Pets.First(p => p.PetId == item.PetId);

                //Update the model from the InputModel
                model = item.UpdateModel(model);

                //Updatet the model in the database
                model.Friend = friend.Item;      //ensure that FriendId can be set, Pet must belong to a friend
                await _pet_service.UpdatePetAsync(new PetCuDto(model));
            }

            return friend.Item;
        }
        private async Task<IFriend> SaveQuotes()
        {
            // Read the current state from database first
            var currentFriend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);
            var existingQuoteIds = currentFriend.Item.Quotes?.Select(q => q.QuoteId).ToHashSet() ?? new HashSet<Guid>();

            //Check if there are deleted quotes, if so simply remove them
            var deletedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedQuotes)
            {
                // Only try to delete if it actually exists in the database
                if (existingQuoteIds.Contains(item.QuoteId))
                {
                    //Remove from the database
                    await _quote_service.DeleteQuoteAsync(item.QuoteId);
                }
            }

            //Check if there are any new quotes added, if so create them in the database
            var newQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Inserted));
            foreach (var item in newQuotes)
            {
                //Create the corresposning model and CUdto objects
                var cuDto = item.CreateCUdto();

                //Set the relationships of a newly created item and write to database
                cuDto.FriendsId = new List<Guid> { FriendInput.FriendId };

                //Create if does not exists. 
                await _quote_service.CreateQuoteAsync(cuDto);
            }

            //To update modified and deleted Quotes, lets first read the original
            //Note that now the deleted quotes will be removed and created quotes will be nicely included
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);


            //Check if there are any modified quotes , if so update them in the database
            var modifiedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedQuotes)
            {
                var model = friend.Item.Quotes.First(q => q.QuoteId == item.QuoteId);

                //Update the model from the InputModel
                model = item.UpdateModel(model);

                //Updatet the model in the database
                await _quote_service.UpdateQuoteAsync(new QuoteCuDto(model));
            }

            return friend.Item;
        }
        #endregion

        #region Constructors
        //Inject services just like in WebApi
        public EditFriendModel(IFriendsService friend_service, IPetsService pet_service,
                              IQuotesService quote_service, IAddressesService address_service, ILogger<EditFriendModel> logger)
        {
            _friend_service = friend_service;
            _pet_service = pet_service;
            _quote_service = quote_service;
            _address_service = address_service;
            _logger = logger;
        }
        #endregion

        #region Input Model
        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        //These classes are in center of ModelBinding and Validation
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted}
        public class PetIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid PetId { get; set; }

            [Required(ErrorMessage = "You must provide a pet name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "You must provide a pet kind")]
            public string Kind { get; set; }

            //This is because I want to confirm modifications in PostEditPet
            [Required(ErrorMessage = "You must provide a pet name")]
            public string editName { get; set; }

            [Required(ErrorMessage = "You must provide a pet kind")]
            public string editKind { get; set; }

            public PetIM() { }
            public PetIM(PetIM original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Name = original.Name;
                Kind = original.Kind;

                editName = original.editName;
                editKind = original.editKind;
            }
            public PetIM(IPet model)
            {
                StatusIM = StatusIM.Unchanged;
                PetId = model.PetId;
                Name = editName = model.Name;
                Kind = editKind = model.Kind.ToString();
            }
            
            //to update the model in database
            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Name = this.Name;
                model.Kind = (AnimalKind)Enum.Parse(typeof(AnimalKind), this.Kind);
                return model;
            }

            //to create new pet in the database
            public PetCuDto CreateCUdto () => new PetCuDto(){

                PetId = null,
                Name = this.Name,
                Kind = (AnimalKind)Enum.Parse(typeof(AnimalKind), this.Kind)
            };
        }
        public class QuoteIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "You must enter a quote")]
            public string QuoteText { get; set; }

            [Required(ErrorMessage = "You must provide an author")]
            public string Author { get; set; }


            [Required(ErrorMessage = "You must enter a quote")]
            public string editQuoteText { get; set; }

            [Required(ErrorMessage = "You must provide an author")]
            public string editAuthor { get; set; }

            public QuoteIM() { }
            public QuoteIM(QuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                QuoteText = original.QuoteText;
                Author = original.Author;


                editQuoteText = original.editQuoteText;
                editAuthor = original.editAuthor;
            }
            public QuoteIM(IQuote model)
            {
                StatusIM = StatusIM.Unchanged;
                QuoteId = model.QuoteId;
                QuoteText = editQuoteText = model.QuoteText;
                Author = editAuthor = model.Author;
            }
            
            //to update the model in database
            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = this.QuoteId;
                model.QuoteText = this.QuoteText;
                model.Author = this.Author;
                return model;
            }

            //to create new quote in the database
            public QuoteCuDto CreateCUdto () => new QuoteCuDto(){

                QuoteId = null,
                Quote = this.QuoteText,
                Author = this.Author
            };
        }
        public class AddressIM
        {
            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "You must provide a street address")]
            public string StreetAddress { get; set; } = string.Empty;

            [Required(ErrorMessage = "You must provide a zip code")]
            [Range(1, int.MaxValue, ErrorMessage = "Zip code must be a positive number")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "You must provide a city")]
            public string City { get; set; } = string.Empty;

            [Required(ErrorMessage = "You must provide a country")]
            public string Country { get; set; } = string.Empty;

            public AddressIM() { }
            
            public AddressIM(IAddress model)
            {
                if (model != null)
                {
                    AddressId = model.AddressId;
                    StreetAddress = model.StreetAddress;
                    ZipCode = model.ZipCode;
                    City = model.City;
                    Country = model.Country;
                }
            }

            public AddressCuDto ToCUdto()
            {
                return new AddressCuDto
                {
                    AddressId = null,
                    StreetAddress = this.StreetAddress,
                    ZipCode = this.ZipCode,
                    City = this.City,
                    Country = this.Country
                };
            }
        }

        public class FriendIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "You must provide a first name")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "You must provide a last name")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "You must provide a birthday")]
            public DateTime Birthday { get; set; }
            public AddressIM Address { get; set; } = new AddressIM();

            public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();
            public List<PetIM> Pets { get; set; } = new List<PetIM>();

            public FriendIM() 
            {

            }
            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Birthday = model.Birthday ?? DateTime.MinValue;
                Address = model.Address != null ? new AddressIM(model.Address) : new AddressIM();

                Quotes = model.Quotes?.Select(m => new QuoteIM(m)).ToList() ?? new List<QuoteIM>();
                Pets = model.Pets?.Select(m => new PetIM(m)).ToList() ?? new List<PetIM>();
            }

            //to update the model in database
            public IFriend UpdateModel(IFriend model)
            {
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Birthday = this.Birthday;
                // Note: We don't update the Address through the model binding
                // Address can only be changed through the Addresses API
                return model;
            }

            //to create new friend in the database
            public FriendCuDto CreateCUdto () => new (){
                
                FriendId = null,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Birthday = this.Birthday,
                AddressId = this.Address?.AddressId != Guid.Empty ? this.Address?.AddressId : null
            };

            //to allow a new quote being specified and bound in the form
            public QuoteIM NewQuote { get; set; } = new QuoteIM();

            //to allow a new pet being specified and bound in the form
            public PetIM NewPet { get; set; } = new PetIM();

            // Add this method to convert FriendIM to FriendCuDto
            public FriendCuDto ToCUdto()
            {
                return new FriendCuDto
                {
                    FriendId = this.FriendId != Guid.Empty ? this.FriendId : null,
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    Birthday = this.Birthday,
                    AddressId = this.Address?.AddressId != Guid.Empty ? this.Address?.AddressId : null,
                    // Include current pets and quotes IDs to preserve them during update
                    PetsId = this.Pets?.Where(p => p.StatusIM != StatusIM.Deleted && p.StatusIM != StatusIM.Inserted)
                                      .Select(p => p.PetId).ToList(),
                    QuotesId = this.Quotes?.Where(q => q.StatusIM != StatusIM.Deleted && q.StatusIM != StatusIM.Inserted)
                                          .Select(q => q.QuoteId).ToList()
                };
            }
        }
        #endregion
    }
}
