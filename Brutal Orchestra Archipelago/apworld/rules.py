ITEM_ACCESS_RULES = {
        "What are You Doing?": {
            "any_heroes": ["Fennec", "Kleiver", "Cranes", "Rags", "Leviat", "Gospel"],
        },
        "Brutality": {
            "min_from": (["Burnout", "Anton", "Pearl", "Dimitri", "Clive", "Kleiver", "Leviat", "Rags"], 3),
        },
        "So Long Liver": {"heroes": ["LongLiver"]},
        "War Criminal": {"any_heroes": ["Arnold"], "or_items": ["Demon Core"]},
        "Human Resources": {"min_heroes": 3},
        

        # Far/Orp + Hardmode
        "The Trickster": {"hardmode": True}, "The Naked": {"hardmode": True},
        "The Failure": {"hardmode": True}, "The Incinerated": {"hardmode": True},
        "The Terminal": {"hardmode": True}, "The Psychic": {"hardmode": True},
        "The Black Lung": {"hardmode": True}, "The Cougar": {"hardmode": True},
        "The Coward": {"hardmode": True}, "The Messiah": {"hardmode": True},
        "Crisis of Faith": {"hardmode": True}, "Bloodline Drinker": {"hardmode": True},
        "Boom, Headshot": {"hardmode": True}, "Mass Grave Matters": {"hardmode": True},
        "Duke of the Dunes": {"hardmode": True}, "Master of the Mountains": {"hardmode": True},
        "Plenty of Fish in the Desert": {"hardmode": True, "heroes": ["Anton", "ShellyK"]},

        # Garden + герои
        "Health Insurance": {"heroes": ["Burnout"]}, "A Gift?": {"heroes": ["Burnout"]},
        "Rotund Amphibian": {"heroes": ["Fennec"]}, "Gamified Cephalopod": {"heroes": ["Fennec"]},
        "You Can Do It!": {"heroes": ["Anton"]}, "Russki Vampire": {"heroes": ["Anton"]},
        "Extra Stitching": {"heroes": ["Splig"]}, "Pain Killers": {"heroes": ["Splig"]},
        "Lycanthrope's Core": {"heroes": ["Pearl"]}, "Head of Scrybe": {"heroes": ["Pearl"]},
        "Fishing Rod": {"heroes": ["Thype"]}, "Effigy of the Mettle Mother": {"heroes": ["Thype"]},
        "Gilded Mirror": {"heroes": ["Griffin"]}, "Spiked Collar": {"heroes": ["Griffin"]},
        "Someone Else's Wedding Ring": {"heroes": ["Arnold"]}, "Fist Full of Ash": {"heroes": ["Arnold"]},
        "Czech Hedgehog": {"heroes": ["Dimitri"]}, "Cremation": {"heroes": ["Dimitri"]},
        "Deworming Pills": {"heroes": ["LongLiver"]}, "Medical Leches": {"heroes": ["LongLiver"]},
        "Holy Chalice": {"heroes": ["Clive"]}, "Seeds of the Consumed": {"heroes": ["Clive"]},
        "The Jersey": {"heroes": ["Kleiver"]}, "Pontiff's Parade": {"heroes": ["Kleiver"]},
        "Mystery Ration": {"heroes": ["Cranes"]}, "Ol' Stumpy": {"heroes": ["Cranes"]},
        "Iron Necklace": {"heroes": ["Agon"]}, "The Apple": {"heroes": ["Agon"]},
        "Trepanation": {"heroes": ["Rags"]}, "Wheel of Fortune": {"heroes": ["Rags"]},
        "Prussian Blue": {"heroes": ["SmokeStacks"]}, "DDT": {"heroes": ["SmokeStacks"]},
        "Blind Faith": {"heroes": ["Leviat"]}, "Modern Medicine": {"heroes": ["Leviat"]},
        "Divine Mud": {"heroes": ["Bimini"]}, "Opulent Egg": {"heroes": ["Bimini"]},
        "Fear of Gods Above": {"heroes": ["Gospel"]},
        "Sculptur's Tools": {"heroes": ["Gospel"]}, "Gospel's Severed Head": {"heroes": ["Gospel"]},
        "Wels Catfish": {"heroes": ["Mung"]}, "Left Shoe": {"heroes": ["Mung"]},
        "Meatre Worm": {"heroes": ["Mordrake"]}, "Norris!": {"heroes": ["Mordrake"]},
        "Burn-Bottle Batch": {"heroes": ["ShellyK"]}, "Royal Pine": {"heroes": ["ShellyK"]},
        "Coelacanth": {"heroes": ["Formosus"]}, "Sacred Shrub": {"heroes": ["Formosus"]},
        "The Mistake": {"min_heroes": 13},

        # Bronzo chain
        "What the !@#$ Nowak?": {"hardmode": True, "prior": "Bronzo's 2 Cents"},
        "Okay Nowak, Seriously Stop!": {"hardmode": True, "prior": "What the !@#$ Nowak?"},
        "That's it Nowak!": {"hardmode": True, "prior": "Okay Nowak, Seriously Stop!"},
        "Time to Die!": {"hardmode": True, "prior": "That's it Nowak!"},
        "The Shyster": {"prior": "Time to Die!"},

        # Final Quests
        "Mordrake's Untold Tale": {"all_heroes": True, "all_locations": True},
        "Brutal Orchestra": {"all_heroes": True, "all_locations": True},
        "The Director's Final Frame": {"all_heroes": True, "all_locations": True},
    }
