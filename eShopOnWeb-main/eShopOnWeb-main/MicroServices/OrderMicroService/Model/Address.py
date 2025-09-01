class Address:
    def __init__(self, street: str, city: str, state: str, country: str, zipcode: str):
        self._street = street
        self._city = city
        self._state = state
        self._country = country
        self._zipcode = zipcode

    @property
    def street(self) -> str:
        return self._street

    @property
    def city(self) -> str:
        return self._city

    @property
    def state(self) -> str:
        return self._state

    @property
    def country(self) -> str:
        return self._country

    @property
    def zipcode(self) -> str:
        return self._zipcode

    def __repr__(self):
        return (f"Address(street='{self._street}', city='{self._city}', "
                f"state='{self._state}', country='{self._country}', "
                f"zipcode='{self._zipcode}')")
